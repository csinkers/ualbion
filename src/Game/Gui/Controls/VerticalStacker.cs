using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls;

/// <summary>
/// Arranges a list of elements vertically. If Greedy is true, then all child
/// elements will be arranged with the width of the widest element (unless the
/// entire stack is otherwise constrained).
/// </summary>
public class VerticalStacker : UiElement
{
    public VerticalStacker(params IUiElement[] children) : this((IList<IUiElement>)children) { }
    public VerticalStacker(IList<IUiElement> children)
    {
        ArgumentNullException.ThrowIfNull(children);
        foreach(var child in children)
            AttachChild(child);
    }

    public bool Greedy { get; set; } = true;
    public bool ProgressiveOverlap { get; set; } // When true lower rows will be drawn on a higher layer than upper rows, e.g. for the combat grid.

    public override Vector2 GetSize()
    {
        Vector2 size = Vector2.Zero;
        foreach (var child in Children)
        {
            if (child is not IUiElement { IsActive: true } childElement)
                continue;

            var childSize = childElement.GetSize();
            if (childSize.X > size.X)
                size.X = childSize.X;

            size.Y += childSize.Y;
        }

        return size;
    }

    protected override int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        int offset = extents.Y;
        int maxOrder = order;
        foreach(var child in Children)
        {
            if (child is not IUiElement { IsActive: true } childElement)
                continue;

            bool greedy = Greedy || child is IGreedyUiElement;

            var childSize = childElement.GetSize();
            int height = (int)childSize.Y;
            var childExtents = greedy
                ? new Rectangle(extents.X, offset, extents.Width, height)
                : new Rectangle(extents.X + (int)(extents.Width - childSize.X) / 2, offset, (int)childSize.X, height);

            int childOrder = ProgressiveOverlap ? maxOrder + 1 : order + 1;
            maxOrder = Math.Max(maxOrder, func(childElement, childExtents, childOrder, context));

            // Rendering may have altered the size of any text elements, so retrieve it
            // again to ensure correct rendering on the first frame.
            height = (int)childElement.GetSize().Y;
            offset += height;
        }
        return maxOrder;
    }
}