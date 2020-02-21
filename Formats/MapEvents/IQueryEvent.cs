namespace UAlbion.Formats.MapEvents
{
    public interface IQueryEvent : IMapEvent
    {
        QueryType QueryType { get; }
        ushort? FalseEventId { get; }
    }
}