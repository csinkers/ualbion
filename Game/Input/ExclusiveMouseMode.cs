using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using Veldrid;

namespace UAlbion.Game.Input
{
    public class ExclusiveMouseMode : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ExclusiveMouseMode, InputEvent>((x, e) => x.OnInput(e)),
            H<ExclusiveMouseMode, UiSelectedEvent>((x,e) => x.OnSelect(e)),
            H<ExclusiveMouseMode, SetExclusiveMouseModeEvent>((x, e) => x._exclusiveItem = e.ExclusiveElement)
        );

        void OnSelect(UiSelectedEvent e)
        {
            IUiEvent newEvent = new UiHoverEvent();
            foreach (var element in e.FocusedItems.Where(x => x == _exclusiveItem))
                element.Receive(newEvent, this);

            newEvent = new UiBlurEvent();
            foreach (var element in e.BlurredItems.Where(x => x == _exclusiveItem))
                element.Receive(newEvent, this);
        }

        IUiElement _exclusiveItem;

        void OnInput(InputEvent e)
        {
            Raise(new ScreenCoordinateSelectEvent(e.Snapshot.MousePosition, (t, selection) => {}));

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

        public ExclusiveMouseMode() : base(Handlers) { }
    }
}