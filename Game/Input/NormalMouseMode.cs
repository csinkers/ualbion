using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Input
{
    public class NormalMouseMode : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<NormalMouseMode, InputEvent>((x, e) => x.OnInput(e)),
            H<NormalMouseMode, UiSelectedEvent>((x,e) => x.OnSelect(e))
        );

        void OnSelect(UiSelectedEvent e)
        {
            IUiEvent newEvent = new UiHoverEvent();
            foreach (var element in e.FocusedItems)
            {
                if (!newEvent.Propagating) break;
                element.Receive(newEvent, this);
            }

            newEvent = new UiBlurEvent();
            foreach (var element in e.BlurredItems)
            {
                if (!newEvent.Propagating) break;
                element.Receive(newEvent, this);
            }
        }

        void OnInput(InputEvent e)
        {
            IList<(float, Selection)> hits = new List<(float, Selection)>();
            Raise(new ScreenCoordinateSelectEvent(e.Snapshot.MousePosition, (t, selection) => hits.Add((t, selection))));
            var orderedHits = hits.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();

            // Clicks are targeted, releases are broadcast. e.g. if you click and drag a slider and move outside
            // its hover area, then it should switch to "ClickedBlurred". If you then release the button while
            // still outside its hover area and releases were broadcast, it would never receive the release and
            // it wouldn't be able to transition back to Normal
            if(e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && x.Down))
            {
                var clickEvent = new UiLeftClickEvent();
                foreach (var hit in orderedHits)
                {
                    if (!clickEvent.Propagating) break;
                    var component = hit.Target as IComponent;
                    component?.Receive(clickEvent, this);
                }
            }

            if(e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && x.Down))
            {
                var clickEvent = new UiRightClickEvent();
                foreach (var hit in orderedHits)
                {
                    if (!clickEvent.Propagating) break;
                    var component = hit.Target as IComponent;
                    component?.Receive(clickEvent, this);
                }
            }

            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && !x.Down))
                Raise(new UiLeftReleaseEvent());

            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && !x.Down))
                Raise(new UiRightReleaseEvent());
        }

        public NormalMouseMode() : base(Handlers) { }
    }
}