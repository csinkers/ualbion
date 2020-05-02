using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class ContextMenuInputMode : Component
    {
        protected override void Subscribed()
        {
            Raise(new PushMouseModeEvent(MouseMode.ContextMenu));
            base.Subscribed();
        }

        public override void Detach()
        {
            Raise(new PopMouseModeEvent());
            base.Detach();
        }
    }
}
