using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

public interface INpcEvent : IEvent
{
    byte NpcNum { get; }
}