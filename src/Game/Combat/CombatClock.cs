using System;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Combat;

public class CombatClock : Component
{
    readonly CombatClockEvent _event = new();
    int _ticks;
    int _combatTicks;

    float _elapsedTimeThisGameFrame;
    int _ticksRemaining;
    int _stoppedFrames;
    int _totalFastTicks;
    float _stoppedMs;
    Action _pendingContinuation;
    bool _isRunning;

    public CombatClock()
    {
        On<StartCombatClockEvent>(_ => IsRunning = true);
        On<StopCombatClockEvent>(_ => IsRunning = false);
        On<EngineUpdateEvent>(OnEngineUpdate);

        OnAsync<CombatUpdateEvent>((e, c) =>
        {
            if (IsRunning || _pendingContinuation != null)
                return false;

            GameTrace.Log.CombatClockUpdating(e.Cycles);
            _pendingContinuation = c;
            _ticksRemaining = e.Cycles * Var(GameVars.Time.FastTicksPerSlowTick);
            IsRunning = true;
            return true;
        });
    }

    void OnUpdate(FastClockEvent updateEvent)
    {
        _ticks += updateEvent.Frames;
        var ticksPerCombat = Var(GameVars.Time.FastTicksPerSlowTick);
        while (_ticks >= ticksPerCombat)
        {
            _ticks -= ticksPerCombat;
            GameTrace.Log.CombatTick(_combatTicks++);
            Raise(_event);
        }
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
                GameTrace.Log.CombatClockStart(_stoppedFrames, _stoppedMs);
                _stoppedFrames = 0;
                _stoppedMs = 0;
            }
            else GameTrace.Log.CombatClockStop();

            _isRunning = value;
        }
    }

    void OnEngineUpdate(EngineUpdateEvent e)
    {
        ElapsedTime += e.DeltaSeconds;

        if (IsRunning)
        {
            _elapsedTimeThisGameFrame += e.DeltaSeconds;
            var tickDurationSeconds = 1.0f / Var(GameVars.Time.FastTicksPerSecond);

            // If the game was paused for a while don't try and catch up
            if (_elapsedTimeThisGameFrame > 4 * tickDurationSeconds)
                _elapsedTimeThisGameFrame = 4 * tickDurationSeconds;

            while (_elapsedTimeThisGameFrame >= tickDurationSeconds && IsRunning)
            {
                _elapsedTimeThisGameFrame -= tickDurationSeconds;
                RaiseTick();
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
        Raise(new FastClockEvent(1));
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