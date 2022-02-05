using UAlbion.Api;

namespace UAlbion.Formats.MapEvents;

public interface INpcEvent : IEvent
{
    byte NpcNum { get; }
}