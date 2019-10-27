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
        public HorizontalStack(params IUiElement[] args) : this((IList<IUiElement>)args) { }
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

        void VisitChildren(Rectangle extents, Action<IUiElement, Rectangle> action)
        {
            int minWidth = 0;
            int nonFixedCount = 0;
            foreach(var child in Children.OfType<IUiElement>())
            {
                int width = (int)child.GetSize().X;
                if (!(child is IFixedSizeUiElement))
                    nonFixedCount++;
                minWidth += width;
            }

            int spareWidth = extents.Width - minWidth;
            int offset = extents.X;
            foreach (var child in Children.OfType<IUiElement>())
            {
                int width = (int)child.GetSize().X;
                var rect = new Rectangle(offset, extents.Y, width, extents.Height);
                if(!(child is IFixedSizeUiElement))
                    rect = new Rectangle(rect.X, rect.Y, rect.Width + spareWidth / nonFixedCount, rect.Height);
                action(child, rect);
                offset += rect.Width;
            }
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            int maxOrder = order;
            VisitChildren(extents, (x, rect) => maxOrder = Math.Max(maxOrder, x.Render(rect, order + 1, addFunc)));
            return maxOrder;
        }

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            int maxOrder = order;
            if (!extents.Contains((int) uiPosition.X, (int) uiPosition.Y)) 
                return maxOrder;

            VisitChildren(extents, (x, rect) => { maxOrder = Math.Max(maxOrder, x.Select(uiPosition, rect, order + 1, registerHitFunc)); });
            registerHitFunc(order, this);

            return maxOrder;
        }
    }
}