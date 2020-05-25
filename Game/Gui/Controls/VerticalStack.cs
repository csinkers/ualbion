using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls
{
    /// <summary>
    /// Arranges a list of elements vertically. If Greedy is true, then all child
    /// elements will be arranged with the width of the widest element (unless the
    /// entire stack is otherwise constrained).
    /// </summary>
    public class VerticalStack : UiElement
    {
        public VerticalStack(params IUiElement[] children) : this((IList<IUiElement>)children) { }
        public VerticalStack(IList<IUiElement> children)
        {
            foreach(var child in children)
                Children.Add(child);
        }

        public bool Greedy { get; set; } = true;

        public override Vector2 GetSize()
        {
            Vector2 size = Vector2.Zero;
            foreach (var child in Children.OfType<IUiElement>())
            {
                var childSize = child.GetSize();
                if (childSize.X > size.X)
                    size.X = childSize.X;

                size.Y += childSize.Y;
            }

            return size;
        }

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            int offset = extents.Y;
            int maxOrder = order;
            foreach(var child in Children.OfType<IUiElement>())
            {
                var childSize = child.GetSize();
                int height = (int)childSize.Y;
                var childExtents = Greedy
                    ? new Rectangle(extents.X, offset, extents.Width, height)
                    : new Rectangle(extents.X + (int)(extents.Width - childSize.X) / 2, offset, (int)childSize.X, height);

                maxOrder = Math.Max(maxOrder, func(child, childExtents, order + 1));
                // Rendering may have altered the size of any text elements, so retrieve it
                // again to ensure correct rendering on the first frame.
                height = (int)child.GetSize().Y;
                offset += height;
            }
            return maxOrder;
        }
    }
}
