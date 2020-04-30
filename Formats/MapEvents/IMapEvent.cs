namespace UAlbion.Formats.MapEvents
{
    public interface IMapEvent : IContextualEvent
    {
        MapEventType EventType { get; }
    }
}
