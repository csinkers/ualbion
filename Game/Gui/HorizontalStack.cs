using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class HorizontalStack : UiElement
    {
        public HorizontalStack(IList<IUiElement> children) : base(null)
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
                size.X += childSize.X;

                if (childSize.Y > size.Y)
                    size.Y = childSize.Y;
            }

            return size;
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            int offset = extents.X;
            int maxOrder = order;
            foreach(var child in Children.OfType<IUiElement>())
            {
                int width = (int)child.GetSize().X;
                maxOrder = Math.Max(maxOrder, child.Render(new Rectangle(offset, extents.Y, width, extents.Height), order + 1, addFunc));
                offset += width;
            }
            return maxOrder;
        }

        public override void Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return;

            int offset = extents.X;
            foreach(var child in Children.OfType<IUiElement>())
            {
                int width = (int)child.GetSize().X;
                child.Select(uiPosition, new Rectangle(offset, extents.Y, width, extents.Height), order + 1, registerHitFunc);
                offset += width;
            }

            registerHitFunc(order, this);
        }
    }
}