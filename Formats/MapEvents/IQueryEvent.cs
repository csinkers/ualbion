using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public interface IQueryEvent : IMapEvent, IAsyncEvent<bool>
    {
        QueryType QueryType { get; }
    }
}
