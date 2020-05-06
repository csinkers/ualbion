using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input
{
    public class ExclusiveMouseMode : Component
    {

        public ExclusiveMouseMode()
        {
            On<InputEvent>(OnInput);
        }

        void OnInput(InputEvent e)
        {
            Resolve<ISelectionManager>()?.CastRayFromScreenSpace(e.Snapshot.MousePosition, true);

            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && !x.Down))
            {
                Raise(new UiLeftReleaseEvent());
                Raise(new SetMouseModeEvent(MouseMode.Normal));
            }

            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && !x.Down))
            {
                Raise(new UiRightReleaseEvent());
                Raise(new SetMouseModeEvent(MouseMode.Normal));
            }
        }
    }
}
