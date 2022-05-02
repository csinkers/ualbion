using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

public interface IMapEvent : IEvent
{
    MapEventType EventType { get; }
}