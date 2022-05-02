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

    protected virtual int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
    {
        int maxOrder = order;
        foreach (var child in Children)
        {
            if (child is not IUiElement { IsActive: true } childElement)
                continue;

            maxOrder = Math.Max(maxOrder, func(childElement, extents, order + 1));
        }

        return maxOrder;
    }

    public virtual Vector2 GetSize() => GetMaxChildSize();
    public virtual int Render(Rectangle extents, int order) => DoLayout(extents, order,
        (child,childExtents,childOrder) => child.Render(childExtents, childOrder));

    public virtual int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
    {
        if (registerHitFunc == null) throw new ArgumentNullException(nameof(registerHitFunc));
        if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
            return order;

        var maxOrder = DoLayout(extents, order, (x,y,z) => x.Select(uiPosition, y, z, registerHitFunc));
        registerHitFunc(order, this);
        return maxOrder;
    }

    public virtual int Layout(Rectangle extents, int order, LayoutNode parent)
    {
        var node = new LayoutNode(parent, this, extents, order);
        return DoLayout(extents, order,
            (child,childExtents,childOrder) => child.Layout(childExtents, childOrder, node));
    }
}