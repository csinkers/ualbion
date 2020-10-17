using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input
{
    public class ContextMenuMouseMode : Component
    {
        public ContextMenuMouseMode()
        {
            On<InputEvent>(OnInput);
        }

        protected override void Subscribed()
        {
            Raise(new SetCursorEvent(Base.CoreSprite.Cursor));
        }

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

            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && !x.Down))
                Raise(new UiLeftReleaseEvent());

            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && x.Down))
                Raise(new CloseWindowEvent());
        }
    }
}
