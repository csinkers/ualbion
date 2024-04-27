using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

public class SlowClockTimerEvent : GameEvent, IVerboseEvent
{
    public SlowClockTimerEvent(int slowTicks) => SlowTicks = slowTicks;
    public int SlowTicks { get; }
}