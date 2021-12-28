using UAlbion.Api;

namespace UAlbion.Game.Events;

public class ModsLoadedEvent : Event
{
    ModsLoadedEvent() {}
    public static ModsLoadedEvent Instance { get; } = new();
}