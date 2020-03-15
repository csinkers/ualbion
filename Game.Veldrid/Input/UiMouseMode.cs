using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;

namespace UAlbion.Game.Veldrid.Input
{
    public class UiMouseMode : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
        );

        public UiMouseMode() : base(Handlers) { }

        void OnInput(InputEvent e)
        {
            if(e.Snapshot.WheelDelta < 0)
                Raise(new MagnifyEvent(-1));
            if(e.Snapshot.WheelDelta > 0)
                Raise(new MagnifyEvent(1));
        }
    }
}
