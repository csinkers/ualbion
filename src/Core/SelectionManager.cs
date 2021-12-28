using System;
using System.Collections.Generic;
using System.Numerics;
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
        foreach (var hit in hits)
            _selection.Front.Add(hit.Target);

        Distribute(_hoverEvent, hits, HoverSelector);
        Distribute(_blurEvent, _selection.Back, BlurSelector);
    }
}