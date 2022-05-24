using UAlbion.Api.Eventing;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game;

public class SlowClock : Component
{
    int _ticks;

    public SlowClock() => On<FastClockEvent>(OnUpdate);

    void OnUpdate(FastClockEvent updateEvent)
    {
        _ticks += updateEvent.Frames;
        int delta = 0;
        var ticksPerSlow = GetVar(GameVars.Time.FastTicksPerSlowTick);
        while(_ticks >= ticksPerSlow)
        {
            _ticks -= ticksPerSlow;
            delta++;
        }

        if (delta <= 0)
            return;

        Raise(new SlowClockEvent(delta));
    }
}

[Event("slow_clock")]
public class SlowClockEvent : GameEvent, IVerboseEvent
{
    [EventPart("delta")] public int Delta { get; }
    public SlowClockEvent(int delta)
    {
        Delta = delta;
    }
}