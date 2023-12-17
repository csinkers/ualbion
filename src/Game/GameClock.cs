using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Config;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game;

public class GameClock : ServiceComponent<IClock>, IClock
{
    readonly IList<(string, float)> _activeTimers = new List<(string, float)>();
    readonly SetTimeEvent _setTimeEvent = new();
    readonly CycleCacheEvent _cycleCacheEvent = new();
    readonly PostGameUpdateEvent _postGameUpdateEvent = new();
    readonly FastClockEvent _fastClockEvent = new(1);

    float _elapsedTimeThisGameFrame;
    int _ticksRemaining;
    int _stoppedFrames;
    int _totalFastTicks;
    float _stoppedMs;
    Action _pendingContinuation;
    bool _isRunning;

    public GameClock()
    {
        On<StartClockEvent>(_ => IsRunning = true);
        On<StopClockEvent>(_ => IsRunning = false);
        On<ToggleClockEvent>(_ => IsRunning = !IsRunning);
        On<EngineUpdateEvent>(OnEngineUpdate);
        On<StartTimerEvent>(StartTimer);

        OnAsync<GameUpdateEvent>((e, c) =>
        {
            if (IsRunning || _pendingContinuation != null)
                return false;

            GameTrace.Log.ClockUpdating(e.Cycles);
            _pendingContinuation = c;
            _ticksRemaining = e.Cycles * Var(GameVars.Time.FastTicksPerSlowTick);
            IsRunning = true;
            return true;
        });
    }

    public float ElapsedTime { get; private set; }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning == value)
                return;

            if (value)
            {
                GameTrace.Log.ClockStart(_stoppedFrames, _stoppedMs);
                _stoppedFrames = 0;
                _stoppedMs = 0;
            }
            else GameTrace.Log.ClockStop();

            _isRunning = value;
        }
    }

    void StartTimer(StartTimerEvent e) => _activeTimers.Add((e.Id, ElapsedTime + e.IntervalSeconds));

    void OnEngineUpdate(EngineUpdateEvent e)
    {
        ElapsedTime += e.DeltaSeconds;

        for (int i = 0; i < _activeTimers.Count; i++)
        {
            if (!(_activeTimers[i].Item2 <= ElapsedTime))
                continue;

            Raise(new TimerElapsedEvent(_activeTimers[i].Item1));
            _activeTimers.RemoveAt(i);
            i--;
        }

        if (IsRunning)
        {
            var state = Resolve<IGameState>();
            if (state != null)
            {
                var lastGameTime = state.Time;
                var newGameTime = lastGameTime.AddSeconds(e.DeltaSeconds * Var(GameVars.Time.GameSecondsPerSecond));
                _setTimeEvent.Time = newGameTime;
                ((IComponent) state).Receive(_setTimeEvent, this);

                int time = newGameTime.Day * 10000 + newGameTime.Hour * 100 + newGameTime.Minute;
                if (newGameTime.Minute != lastGameTime.Minute)
                {
                    GameTrace.Log.MinuteElapsed(time);
                    Raise(MinuteElapsedEvent.Instance);
                }

                if (newGameTime.Hour != lastGameTime.Hour)
                {
                    GameTrace.Log.HourElapsed(time);
                    Raise(HourElapsedEvent.Instance);
                }

                if (newGameTime.Date != lastGameTime.Date)
                {
                    GameTrace.Log.DayElapsed(time);
                    Raise(DayElapsedEvent.Instance);
                }
            }

            _elapsedTimeThisGameFrame += e.DeltaSeconds;
            var tickDurationSeconds = 1.0f / Var(GameVars.Time.FastTicksPerSecond);

            // If the game was paused for a while don't try and catch up
            if (_elapsedTimeThisGameFrame > 4 * tickDurationSeconds)
                _elapsedTimeThisGameFrame = 4 * tickDurationSeconds;

            while (_elapsedTimeThisGameFrame >= tickDurationSeconds && IsRunning)
            {
                _elapsedTimeThisGameFrame -= tickDurationSeconds;
                RaiseTick();

                var ticksPerCycle = Var(GameVars.Time.FastTicksPerAssetCacheCycle);
                if ((state?.TickCount ?? 0) % ticksPerCycle == ticksPerCycle - 1)
                    Raise(_cycleCacheEvent);
            }
        }
        else
        {
            _stoppedFrames++;
            _stoppedMs += 1000.0f * e.DeltaSeconds;
        }

        Raise(_postGameUpdateEvent);
    }

    void RaiseTick()
    {
        GameTrace.Log.FastTick(_totalFastTicks++);
        Raise(_fastClockEvent);
        if (_ticksRemaining <= 0)
            return;

        _ticksRemaining --;
        if (_ticksRemaining > 0)
            return;

        IsRunning = false;
        GameTrace.Log.ClockUpdateComplete();
        var continuation = _pendingContinuation;
        _pendingContinuation = null;
        continuation?.Invoke();
    }
}