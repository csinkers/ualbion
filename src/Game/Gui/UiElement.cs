using System;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui;

/// <summary>
/// Abstract base class providing reasonable defaults for the methods
/// on IUiElement. By default the size will be that of the largest
/// child element, and when calculating layout the full extents
/// will be passed through to each child.
/// </summary>
public abstract class UiElement : GameComponent, IUiElement
{
    protected Vector2 GetMaxChildSize()
    {
        if (Children == null) 
            return Vector2.Zero;

        Vector2 size = Vector2.Zero;
        for (var index = 0; index < Children.Count; index++)
        {
            var child = Children[index];
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

    // Cached delegates to avoid per-call allocations on higher order functions
    protected delegate int LayoutFunc<in T>(IUiElement element, Rectangle extents, int order, T context);
    protected static readonly LayoutFunc<LayoutNode> RenderChildDelegate = RenderChild;
    protected static readonly LayoutFunc<SelectionContext> SelectChildDelegate = SelectChild;

    protected static int RenderChild(IUiElement child, Rectangle extents, int order, LayoutNode parent)
    {
        ArgumentNullException.ThrowIfNull(child);
        return child.Render(extents, order, parent);
    }

    protected static int SelectChild(IUiElement child, Rectangle extents, int order, SelectionContext context)
    {
        ArgumentNullException.ThrowIfNull(child);
        return child.Selection(extents, order, context);
    }

    protected virtual int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        int maxOrder = order;
        for (var index = 0; index < Children.Count; index++)
        {
            var child = Children[index];
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
        return DoLayout(extents, order, node, RenderChildDelegate);
    }

    public virtual int Selection(Rectangle extents, int order, SelectionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!extents.Contains((int)context.UiPosition.X, (int)context.UiPosition.Y))
            return order;

        var maxOrder = DoLayout(extents, order, context, (x,y,z, context) => x.Selection(y, z, context));
        context.AddHit(order, this);
        return maxOrder;
    }
}