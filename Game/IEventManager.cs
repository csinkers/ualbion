using UAlbion.Formats.MapEvents;

namespace UAlbion.Game
{
    public interface IEventManager
    {
        EventContext Context { get; }
        bool LastEventResult { get; set; }
    }
}