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
        public DebugPickMouseMode()
        {
            On<InputEvent>(OnInput);
        }

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
    }
}
