using System.Linq;
using SerdesNet;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input
{
    public class ExclusiveMouseMode : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ExclusiveMouseMode, InputEvent>((x, e) => x.OnInput(e)),
            H<ExclusiveMouseMode, SetExclusiveMouseModeEvent>((x, e) => x._exclusiveItem = e.ExclusiveElement)
        );

        public ExclusiveMouseMode() : base(Handlers) { }

        IUiElement _exclusiveItem;

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
