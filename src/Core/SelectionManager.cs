using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;

namespace UAlbion.Core;

public class SelectionManager : ServiceComponent<ISelectionManager>, ISelectionManager
{
    readonly BlurEvent _blurEvent = new();
    readonly HoverEvent _hoverEvent = new();
    readonly DoubleBuffered<HashSet<object>> _selection = new(() => new HashSet<object>());

    IComponent HoverSelector(Selection o)
    {
        if (_selection.Back.Contains(o.Target))
            return null;

        return o.Target as IComponent;
    }

    IComponent BlurSelector(object o)
    {
        if (_selection.Front.Contains(o))
            return null;

        return o as IComponent;
    }

    public void CastRayFromScreenSpace(List<Selection> hits, Vector2 pixelPosition, bool debug, bool performFocusAlerts)
    {
        if (hits == null) throw new ArgumentNullException(nameof(hits));
        RaiseAsync(new ScreenCoordinateSelectEvent(pixelPosition, debug), hits.Add);
        hits.Sort(static (x, y) => x.Distance.CompareTo(y.Distance));

        if (!performFocusAlerts)
            return;

        _selection.Swap();
        _selection.Front.Clear();

        for (int index = 0; index < hits.Count; index++)
        {
            var hit = hits[index];
            _selection.Front.Add(hit.Target);

            if (hit.Target is ISelectionBlocker)
            {
                hits.RemoveRange(index + 1, hits.Count - index - 1);
                break;
            }
        }

        Distribute(_hoverEvent, hits, HoverSelector);
        Distribute(_blurEvent, _selection.Back, BlurSelector);
    }
}