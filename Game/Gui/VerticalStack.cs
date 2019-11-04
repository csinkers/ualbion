using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
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

        int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            int offset = extents.Y;
            int maxOrder = order;
            foreach(var child in Children.OfType<IUiElement>())
            {
                int height = (int)child.GetSize().Y;
                var childExtents = Greedy
                    ? new Rectangle(extents.X, offset, extents.Width, height)
                    : new Rectangle(extents.X, offset, (int)child.GetSize().X, height);

                maxOrder = Math.Max(maxOrder, func(child, childExtents, order + 1));
                offset += height;
            }
            return maxOrder;
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc) => DoLayout(extents, order, (x, y, z) => x.Render(y, z, addFunc));
        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return order;

            return DoLayout(extents, order, (x, y, z) => x.Select(uiPosition, y, z, registerHitFunc));
        }
    }
}