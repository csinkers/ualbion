using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input
{
    public class NormalMouseMode : Component
    {
        Vector2 _lastPosition;
        public NormalMouseMode()
        {
            On<InputEvent>(OnInput);
        }

        protected override void Subscribed()
        {
            Raise(new SetCursorEvent(CoreSpriteId.Cursor));
        }

        void OnInput(InputEvent e)
        {
            var hits = Resolve<ISelectionManager>()?.CastRayFromScreenSpace(e.Snapshot.MousePosition, true);

            // Clicks are targeted, releases are broadcast. e.g. if you click and drag a slider and move outside
            // its hover area, then it should switch to "ClickedBlurred". If you then release the button while
            // still outside its hover area and releases were broadcast, it would never receive the release and
            // it wouldn't be able to transition back to Normal
            if (hits != null)
            {
                ICancellableEvent clickEvent = null;
                if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && x.Down))
                    clickEvent = new UiRightClickEvent();

                if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && x.Down))
                    clickEvent = new UiLeftClickEvent();

                if (clickEvent != null)
                {
                    foreach (var hit in hits)
                    {
                        if (!clickEvent.Propagating) break;
                        var component = hit.Target as IComponent;
                        component?.Receive(clickEvent, this);
                    }
                }
            }

            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && !x.Down))
                Raise(new UiLeftReleaseEvent());

            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && !x.Down))
                Raise(new UiRightReleaseEvent());

            if(_lastPosition != e.Snapshot.MousePosition)
            {
                _lastPosition = e.Snapshot.MousePosition;
                var window = Resolve<IWindowManager>();
                var uiPosition = window.NormToUi(window.PixelToNorm(e.Snapshot.MousePosition));
                Raise(new UiMouseMoveEvent((int)uiPosition.X, (int)uiPosition.Y));
            }
        }
    }
}
