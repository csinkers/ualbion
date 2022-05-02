using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("minute_elapsed")]
public class MinuteElapsedEvent : GameEvent, IVerboseEvent
{
    public static MinuteElapsedEvent Instance { get; } = new();
}