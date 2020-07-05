using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class SelectionManager : ServiceComponent<ISelectionManager>, ISelectionManager
    {
        HashSet<object> _lastSelection = new HashSet<object>();

        public IList<Selection> CastRayFromScreenSpace(Vector2 pixelPosition, bool performFocusAlerts)
        {
            IList<Selection> hits = new List<Selection>();
            RaiseAsync(new ScreenCoordinateSelectEvent(pixelPosition), x => hits.Add(x));
            var orderedHits = hits.OrderBy(x => x.Distance).ToList();

            if (!performFocusAlerts)
                return orderedHits;

            var newSelection = orderedHits.Select(x => x.Target).ToHashSet();
            var focused = newSelection.Except(_lastSelection);
            var blurred = _lastSelection.Except(newSelection);
            Distribute(new BlurEvent(), blurred);
            Distribute(new HoverEvent(), focused);
            _lastSelection = newSelection;
            return orderedHits;
        }

        void Distribute(ICancellableEvent e, IEnumerable<object> targets)
        {
            foreach (var component in targets.OfType<IComponent>())
            {
                if (!e.Propagating) break;
                component.Receive(e, this);
            }
        }
    }
}
