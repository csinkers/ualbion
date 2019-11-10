using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class VerticalStack : UiElement
    {
        public VerticalStack(params IUiElement[] children) : this((IList<IUiElement>)children) { }
        public VerticalStack(IList<IUiElement> children) : base(null)
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