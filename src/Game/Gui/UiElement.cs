using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core;

namespace UAlbion.Game.Gui;

/// <summary>
/// Abstract base class providing reasonable defaults for the methods
/// on IUiElement. By default the size will be that of the largest
/// child element, and when calculating layout the full extents
/// will be passed through to each child.
/// </summary>
public abstract class UiElement : Component, IUiElement
{
    protected Vector2 GetMaxChildSize()
    {
        if (Children == null) 
            return Vector2.Zero;

        Vector2 size = Vector2.Zero;
        foreach (var child in Children)
        {
            if (child is not IUiElement { IsActive: true } childElement)
                continue;

            var childSize = childElement.GetSize();
            if (childSize.X > size.X)
                size.X = childSize.X;
            if (childSize.Y > size.Y)
                size.Y = childSize.Y;
        }
        return size;
    }

    protected delegate int LayoutFunc<in T>(IUiElement element, Rectangle extents, int order, T context);
    protected static int RenderChild(IUiElement child, Rectangle extents, int order, LayoutNode parent) => child.Render(extents, order, parent);
    protected static int SelectChild(IUiElement child, Rectangle extents, int order, SelectionContext context) => child.Select(extents, order, context);
    protected virtual int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        int maxOrder = order;
        foreach (var child in Children)
        {
            if (child is not IUiElement { IsActive: true } childElement)
                continue;

            maxOrder = Math.Max(maxOrder, func(childElement, extents, order + 1, context));
        }

        return maxOrder;
    }

    public virtual Vector2 GetSize() => GetMaxChildSize();

    public virtual int Render(Rectangle extents, int order, LayoutNode parent)
    {
        var node = parent == null ? null : new LayoutNode(parent, this, extents, order);
        return DoLayout(extents, order, node, RenderChild);
    }

    public virtual int Select(Rectangle extents, int order, SelectionContext c)
    {
        if (!extents.Contains((int)c.UiPosition.X, (int)c.UiPosition.Y))
            return order;

        var maxOrder = DoLayout(extents, order, c, (x,y,z, context) => x.Select(y, z, context));
        c.HitFunc(order, this);
        return maxOrder;
    }
}