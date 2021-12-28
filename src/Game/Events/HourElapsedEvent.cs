using UAlbion.Api;

namespace UAlbion.Game.Events;

[Event("hour_elapsed")]
public class HourElapsedEvent : GameEvent
{
    public static HourElapsedEvent Instance { get; } = new();
}