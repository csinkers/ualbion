using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class ContextMenuInputMode : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ContextMenuInputMode, ExchangeDisabledEvent>((x, e) => x.Unsubscribed())
        );

        public ContextMenuInputMode() : base(Handlers) { }

        public override void Subscribed()
        {
            Raise(new PushMouseModeEvent(MouseMode.ContextMenu));
            base.Subscribed();
        }

        void Unsubscribed() => Raise(new PopMouseModeEvent());
    }
}