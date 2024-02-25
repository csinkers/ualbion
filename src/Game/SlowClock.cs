using UAlbion.Api.Eventing;
using UAlbion.Game.Events;

namespace UAlbion.Game;

[Event("slow_clock")] public class SlowClockEvent : GameEvent, IVerboseEvent { }
public class SlowClock : Component
{
    readonly SlowClockEvent _event = new();
    int _ticks;
    int _slowTicks;

    public SlowClock() => On<FastClockEvent>(OnUpdate);

    void OnUpdate(FastClockEvent updateEvent)
    {
        _ticks += updateEvent.Frames;
        var ticksPerSlow = ReadVar(V.Game.Time.FastTicksPerSlowTick);
        while (_ticks >= ticksPerSlow)
        {
            _ticks -= ticksPerSlow;
            GameTrace.Log.SlowTick(_slowTicks++);
            Raise(_event);
        }
    }
}
