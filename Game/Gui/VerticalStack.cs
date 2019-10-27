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

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            int offset = extents.Y;
            int maxOrder = order;
            foreach(var child in Children.OfType<IUiElement>())
            {
                int height = (int)child.GetSize().Y;
                maxOrder = Math.Max(maxOrder, child.Render(new Rectangle(extents.X,  offset, extents.Width, height), order + 1, addFunc));
                offset += height;
            }
            return maxOrder;
        }

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return order;

            int offset = extents.Y;
            int maxOrder = order;
            foreach(var child in Children.OfType<IUiElement>())
            {
                int height = (int)child.GetSize().Y;
                maxOrder = Math.Max(maxOrder, child.Select(uiPosition, new Rectangle(extents.X,  offset, extents.Width, height), order + 1, registerHitFunc));
                offset += height;
            }

            registerHitFunc(order, this);
            return maxOrder;
        }
    }
}