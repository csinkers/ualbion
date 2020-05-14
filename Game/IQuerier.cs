using UAlbion.Formats.MapEvents;

namespace UAlbion.Game
{
    public interface IQuerier
    {
        void Query(IQueryEvent query);
        bool? QueryDebug(IQueryEvent query);
    }
}
