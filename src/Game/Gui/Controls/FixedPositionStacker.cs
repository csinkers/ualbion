using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls;

public class FixedPositionStacker : UiElement, IFixedSizeUiElement
{
    readonly IList<Child> _positions = new List<Child>();

    class Child
    {
        public Child(IUiElement element, int x, int y, int? width, int? height)
        {
            Element = element;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public IUiElement Element { get; }
        public int X { get; set; }
        public int Y { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public override string ToString() => $"{Element} @ <{X}, {Y}>";
    }

    public FixedPositionStacker Add(IUiElement child, int x, int y)
    {
        _positions.Add(new Child(child, x, y, null, null));
        AttachChild(child);
        return this;
    }

    public FixedPositionStacker Add(IUiElement child, int x, int y, int w, int h)
    {
        _positions.Add(new Child(child, x, y, w, h));
        AttachChild(child);
        return this;
    }

    public void Move(IUiElement element, int x, int y, int? w = null, int? h = null)
    {
        foreach (var child in _positions)
        {
            if (child.Element != element) 
                continue;

            child.X = x;
            child.Y = y;
            child.Width = w ?? child.Width;
            child.Height = h ?? child.Height;
            break;
        }
    }

    public override Vector2 GetSize()
    {
        if(!_positions.Any())
            return Vector2.Zero;

        var size = Vector2.Zero;
        foreach(var child in _positions)
        {
            var childSize = child.Element.GetSize();
            childSize.X = child.Width ?? childSize.X;
            childSize.Y = child.Height ?? childSize.Y;

            childSize += new Vector2(child.X, child.Y);

            size.X = Math.Max(size.X, childSize.X);
            size.Y = Math.Max(size.Y, childSize.Y);
        }

        return size;
    }

    protected override int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        int maxOrder = order;
        foreach (var child in _positions)
        {
            var childSize = child.Width.HasValue && child.Height.HasValue ? Vector2.Zero : child.Element.GetSize();
            var childExtents = new Rectangle(
                extents.X + child.X,
                extents.Y + child.Y,
                (int)(child.Width ?? childSize.X),
                (int)(child.Height ?? childSize.Y));

            maxOrder = Math.Max(maxOrder, func(child.Element, childExtents, order + 1, context));
        }
        return maxOrder;
    }

    public override int Selection(Rectangle extents, int order, SelectionContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        // The fixed positions may be outside the regular UI area, so don't clip to the extents that are passed in.
        var maxOrder = DoLayout(extents, order, context, SelectChild);
        context.AddHit(order, this);
        return maxOrder;
    }
}