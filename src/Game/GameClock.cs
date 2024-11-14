using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game;

public class GameClock : ServiceComponent<IClock>, IClock
{
    readonly List<(AlbionTaskCore Task, float ExpiryTimeSeconds)> _activeTimers = [];
    readonly SetTimeEvent _setTimeEvent = new();
    readonly CycleCacheEvent _cycleCacheEvent = new();
    readonly FastClockEvent _fastClockEvent = new(1);

    AlbionTaskCore _currentUpdate;
    float _elapsedTimeThisGameFrame;
    int _ticksRemaining;
    int _stoppedFrames;
    int _totalFastTicks;
    float _stoppedMs;
    bool _isRunning;

    public GameClock()
    {
        On<StartClockEvent>(_ => IsRunning = true);
        On<StopClockEvent>(_ => IsRunning = false);
        On<ToggleClockEvent>(_ => IsRunning = !IsRunning);
        On<EngineUpdateEvent>(OnEngineUpdate);
        OnAsync<WallClockTimerEvent>(StartTimer);

        OnAsync<GameUpdateEvent>(e =>
        {
            if (IsRunning) { Warn($"Ignoring {e} - clock paused"); return AlbionTask.CompletedTask; }
            if (_currentUpdate != null) { Warn($"Ignoring {e} - already running another update event"); return AlbionTask.CompletedTask; }

            GameTrace.Log.ClockUpdating(e.Cycles);
            _currentUpdate = new AlbionTaskCore("GameClock.GameUpdateEvent");
            _ticksRemaining = e.Cycles * ReadVar(V.Game.Time.FastTicksPerSlowTick);
            IsRunning = true;
            return _currentUpdate.UntypedTask;
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

    AlbionTask StartTimer(WallClockTimerEvent e)
    {
        var atc = new AlbionTaskCore();
        _activeTimers.Add((atc, ElapsedTime + e.IntervalSeconds));
        return atc.UntypedTask;
    }

    void OnEngineUpdate(EngineUpdateEvent e)
    {
        ElapsedTime += e.DeltaSeconds;

        for (int i = 0; i < _activeTimers.Count; i++)
        {
            if (ElapsedTime < _activeTimers[i].ExpiryTimeSeconds)
                continue;

            var task = _activeTimers[i].Task;
            _activeTimers.RemoveAt(i);
            i--;

            task.Complete();
        }

        if (IsRunning)
        {
            var state = Resolve<IGameState>();
            if (state != null)
            {
                var lastGameTime = state.Time;
                var newGameTime = lastGameTime.AddSeconds(e.DeltaSeconds * ReadVar(V.Game.Time.GameSecondsPerSecond));
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
            var tickDurationSeconds = 1.0f / ReadVar(V.Game.Time.FastTicksPerSecond);

            // If the game was paused for a while don't try and catch up
            if (_elapsedTimeThisGameFrame > 4 * tickDurationSeconds)
                _elapsedTimeThisGameFrame = 4 * tickDurationSeconds;

            while (_elapsedTimeThisGameFrame >= tickDurationSeconds && IsRunning)
            {
                _elapsedTimeThisGameFrame -= tickDurationSeconds;
                RaiseTick();

                var ticksPerCycle = ReadVar(V.Game.Time.FastTicksPerAssetCacheCycle);
                if ((state?.TickCount ?? 0) % ticksPerCycle == ticksPerCycle - 1)
                    Raise(_cycleCacheEvent);
            }
        }
        else
        {
            _stoppedFrames++;
            _stoppedMs += 1000.0f * e.DeltaSeconds;
        }
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

        var currentUpdate = _currentUpdate;
        _currentUpdate = null;
        currentUpdate.Complete();
    }
}
