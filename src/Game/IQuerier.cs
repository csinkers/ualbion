using UAlbion.Formats.MapEvents;

namespace UAlbion.Game
{
    public interface IQuerier
    {
        bool? QueryDebug(QueryEvent query);
    }
}
