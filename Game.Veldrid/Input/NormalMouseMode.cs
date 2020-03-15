using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input
{
    public class NormalMouseMode : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<NormalMouseMode, InputEvent>((x, e) => x.OnInput(e))
        );

        public override void Subscribed()
        {
            Raise(new SetCursorEvent(CoreSpriteId.Cursor));
            base.Subscribed();
        }

        void OnInput(InputEvent e)
        {
            var hits = Resolve<ISelectionManager>()?.CastRayFromScreenSpace(e.Snapshot.MousePosition, true);

            // Clicks are targeted, releases are broadcast. e.g. if you click and drag a slider and move outside
            // its hover area, then it should switch to "ClickedBlurred". If you then release the button while
            // still outside its hover area and releases were broadcast, it would never receive the release and
            // it wouldn't be able to transition back to Normal
            if (hits != null && e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && x.Down))
            {
                var clickEvent = new UiLeftClickEvent();
                foreach (var hit in hits)
                {
                    if (!clickEvent.Propagating) break;
                    var component = hit.Target as IComponent;
                    component?.Receive(clickEvent, this);
                }
            }

            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && x.Down))
                Raise(new PushMouseModeEvent(MouseMode.RightButtonHeld));

            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && !x.Down))
                Raise(new UiLeftReleaseEvent());
        }

        public NormalMouseMode() : base(Handlers) { }
    }
}
