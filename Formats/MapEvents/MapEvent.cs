using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public abstract class MapEvent : Event, IMapEvent
    {
        public abstract MapEventType EventType { get; }
        public EventContext Context { get; set; }
    }
}
