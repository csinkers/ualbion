using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("hour_elapsed")]
public class HourElapsedEvent : GameEvent
{
    public static HourElapsedEvent Instance { get; } = new();
}