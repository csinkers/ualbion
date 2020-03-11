using UAlbion.Formats.Config;
using UAlbion.Game.Gui;

namespace UAlbion.Game.Events
{
    public class SetExclusiveMouseModeEvent : GameEvent, ISetMouseModeEvent
    {
        public IUiElement ExclusiveElement { get; }
        public MouseMode Mode => MouseMode.Exclusive;

        public SetExclusiveMouseModeEvent(IUiElement exclusiveElement)
        {
            ExclusiveElement = exclusiveElement;
        }
    }
}
