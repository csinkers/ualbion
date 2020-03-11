using SerdesNet;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Input
{
    public class World2DInputMode : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<World2DInputMode, InputEvent>((x,e) => x.OnInput(e)),
            H<World2DInputMode, ExchangeDisabledEvent>((x,e) => x.Unsubscribed())
        );

        public override void Subscribed()
        {
            Raise(new PushMouseModeEvent(MouseMode.Normal));
            base.Subscribed();
        }

        void Unsubscribed() => Raise(new PopMouseModeEvent());

        void OnInput(InputEvent e)
        {
            /*
            if(e.Snapshot.WheelDelta < 0)
                Raise(new MagnifyEvent(-1));
            if(e.Snapshot.WheelDelta > 0)
                Raise(new MagnifyEvent(1));
                */
        }

        public World2DInputMode() : base(Handlers) { }
    }
}
