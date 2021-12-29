using UAlbion.Api;

namespace UAlbion.Game.Events;

public class ModsLoadedEvent : Event, IVerboseEvent
{
    ModsLoadedEvent() {}
    public static ModsLoadedEvent Instance { get; } = new();
}