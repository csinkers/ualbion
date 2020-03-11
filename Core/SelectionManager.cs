using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class SelectionManager : Component, ISelectionManager
    {
        HashSet<object> _lastSelection = new HashSet<object>();

        public IList<Selection> CastRayFromScreenSpace(Vector2 pixelPosition, bool performFocusAlerts)
        {
            IList<(float, Selection)> hits = new List<(float, Selection)>();
            Raise(new ScreenCoordinateSelectEvent(pixelPosition, (t, selection) => hits.Add((t, selection))));
            var orderedHits = hits.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();

            if (performFocusAlerts)
            {
                var newSelection = orderedHits.Select(x => x.Target).ToHashSet();
                var focused = newSelection.Except(_lastSelection);
                var blurred = _lastSelection.Except(newSelection);
                _lastSelection = newSelection;

                ICancellableEvent newEvent = new BlurEvent();
                foreach (var component in blurred.OfType<IComponent>())
                {
                    if (!newEvent.Propagating) break;
                    component.Receive(newEvent, this);
                }

                newEvent = new HoverEvent();
                foreach (var component in focused.OfType<IComponent>())
                {
                    if (!newEvent.Propagating) break;
                    component.Receive(newEvent, this);
                }
            }

            return orderedHits;
        }
    }
}
