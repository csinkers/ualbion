using UAlbion.Api;
using UAlbion.Game.Text;

namespace UAlbion.Game.Events
{
    public class HoverTextEvent : GameEvent, IVerboseEvent
    {
        public HoverTextEvent(IText source) { Source = source; }
        public IText Source { get; }
    }
}
