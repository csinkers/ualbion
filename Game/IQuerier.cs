using UAlbion.Formats.MapEvents;

namespace UAlbion.Game
{
    public interface IQuerier
    {
        bool Query(EventChainContext context, IQueryEvent query);
    }
}