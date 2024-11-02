using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Game.Events;

namespace UAlbion.Game.Combat;

public class CombatClock : Component
{
    readonly CombatClockEvent _event = new();
    readonly FastClockEvent _fastClockEvent = new(1);
    int _ticks;
    int _combatTicks;

    AlbionTaskCore _currentUpdate;
    float _elapsedTimeThisGameFrame;
    int _ticksRemaining;
    int _stoppedFrames;
    int _totalFastTicks;
    float _stoppedMs;
    bool _isRunning;

    public CombatClock()
    {
        On<StartCombatClockEvent>(_ => IsRunning = true);
        On<StopCombatClockEvent>(_ => IsRunning = false);
        On<EngineUpdateEvent>(OnEngineUpdate);

        OnAsync<CombatUpdateEvent>(e =>
        {
            if (IsRunning) { Warn($"Ignoring {e} - clock paused"); return AlbionTask.CompletedTask; }
            if (_currentUpdate != null) { Warn($"Ignoring {e} - already running another update event"); return AlbionTask.CompletedTask; }

            GameTrace.Log.CombatClockUpdating(e.Cycles);
            _currentUpdate = new AlbionTaskCore("CombatClock.CombatUpdateEvent");
            _ticksRemaining = e.Cycles * ReadVar(V.Game.Time.FastTicksPerSlowTick);
            IsRunning = true;
            return _currentUpdate.UntypedTask;
        });
    }

    void OnUpdate()
    {
        _ticks++;
        var ticksPerCombat = ReadVar(V.Game.Time.FastTicksPerSlowTick);
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
            var tickDurationSeconds = 1.0f / ReadVar(V.Game.Time.FastTicksPerSecond);

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
        Raise(_fastClockEvent);
        OnUpdate();

        if (_ticksRemaining <= 0)
            return;

        _ticksRemaining--;

        if (_ticksRemaining == 0)
        {
            IsRunning = false;
            GameTrace.Log.ClockUpdateComplete();

            var currentUpdate = _currentUpdate;
            _currentUpdate = null;
            currentUpdate.Complete();
        }
    }
}
