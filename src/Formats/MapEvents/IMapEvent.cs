using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public interface IMapEvent : IEvent
    {
        MapEventType EventType { get; }
    }
}
