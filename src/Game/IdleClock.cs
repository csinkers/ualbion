using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game;

[Event("idle")] public class IdleClockEvent : GameEvent, IVerboseEvent { }
public class IdleClock : Component
{
    readonly IdleClockEvent _event = new();
    float _elapsedTimeThisGameFrame;

    public IdleClock()
    {
        On<EngineUpdateEvent>(OnEngineUpdate);
    }

    void OnEngineUpdate(EngineUpdateEvent e)
    {
        _elapsedTimeThisGameFrame += e.DeltaSeconds;
        var config = Resolve<IGameConfigProvider>().Game;
        var tickDurationSeconds = 1.0f / config.Time.IdleTicksPerSecond;

        // If the game was paused for a while don't try and catch up
        if (_elapsedTimeThisGameFrame > 4 * tickDurationSeconds)
            _elapsedTimeThisGameFrame = 4 * tickDurationSeconds;

        while (_elapsedTimeThisGameFrame >= tickDurationSeconds)
        {
            _elapsedTimeThisGameFrame -= tickDurationSeconds;
            Raise(_event);
        }
    }
}