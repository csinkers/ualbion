using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Game.Events;

namespace UAlbion.Game;

[Event("idle")] public class IdleClockEvent : GameEvent, IVerboseEvent { }
public class IdleClock : Component
{
    readonly IdleClockEvent _event = new();
    float _elapsedTimeThisGameFrame;
    int _total;

    public IdleClock()
    {
        On<EngineUpdateEvent>(OnEngineUpdate);
    }

    void OnEngineUpdate(EngineUpdateEvent e)
    {
        _elapsedTimeThisGameFrame += e.DeltaSeconds;
        var tickDurationSeconds = 1.0f / ReadVar(V.Game.Time.IdleTicksPerSecond);

        // If the game was paused for a while don't try and catch up
        if (_elapsedTimeThisGameFrame > 4 * tickDurationSeconds)
            _elapsedTimeThisGameFrame = 4 * tickDurationSeconds;

        while (_elapsedTimeThisGameFrame >= tickDurationSeconds)
        {
            _elapsedTimeThisGameFrame -= tickDurationSeconds;
            GameTrace.Log.IdleTick(_total++);
            Raise(_event);
        }
    }
}