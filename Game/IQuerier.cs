using UAlbion.Formats.MapEvents;

namespace UAlbion.Game
{
    public interface IQuerier
    {
        bool Query(EventContext context, IQueryEvent query);
    }
}
