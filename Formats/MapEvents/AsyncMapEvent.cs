using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public abstract class AsyncMapEvent : AsyncEvent, IMapEvent
    {
        public abstract MapEventType EventType { get; }
    }
}