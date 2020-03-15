using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input
{
    public class DebugPickMouseMode : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<DebugPickMouseMode, InputEvent>((x,e) => x.OnInput(e))
        );

        void OnInput(InputEvent e)
        {
            if(e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && x.Down))
            {
                Raise(new PopMouseModeEvent());
                return;
            }

            var hits = Resolve<ISelectionManager>()?.CastRayFromScreenSpace(e.Snapshot.MousePosition, true);
            Raise(new ShowDebugInfoEvent(hits, e.Snapshot.MousePosition));
        }

        public DebugPickMouseMode() : base(Handlers) { }
    }
}
