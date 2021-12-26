using System.Collections.Generic;
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
        readonly List<Selection> _hits = new();
        readonly PopMouseModeEvent _popMouseModeEvent = new();

        public DebugPickMouseMode() => On<InputEvent>(OnInput);

        void OnInput(InputEvent e)
        {
            if(e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && x.Down))
            {
                Raise(_popMouseModeEvent);
                return;
            }

            _hits.Clear();
            Resolve<ISelectionManager>()?.CastRayFromScreenSpace(_hits, e.Snapshot.MousePosition, true, true);
            Raise(new ShowDebugInfoEvent(_hits, e.Snapshot.MousePosition));
        }
    }
}
