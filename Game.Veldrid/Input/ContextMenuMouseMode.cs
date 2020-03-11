using System.Linq;
using SerdesNet;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input
{
    public class ContextMenuMouseMode : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ContextMenuMouseMode, InputEvent>((x,e) => x.OnInput(e))
        );

        void OnInput(InputEvent e)
        {
            var hits = Resolve<ISelectionManager>()?.CastRayFromScreenSpace(e.Snapshot.MousePosition, true);
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
                Raise(new CloseDialogEvent());
        }

        public ContextMenuMouseMode() : base(Handlers) { }
        public override void Subscribed()
        {
            Raise(new SetCursorEvent(CoreSpriteId.Cursor));
            base.Subscribed();
        }
    }
}
