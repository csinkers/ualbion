using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public abstract class UiElement : Component, IUiElement
    {
        protected Vector2 GetMaxChildSize()
        {
            Vector2 size = Vector2.Zero;
            if (Children != null)
            {
                foreach (var child in Children.OfType<IUiElement>())
                {
                    var childSize = child.GetSize();
                    if (childSize.X > size.X)
                        size.X = childSize.X;
                    if (childSize.Y > size.Y)
                        size.Y = childSize.Y;
                }
            }
            return size;
        }

        protected UiElement(IList<Handler> handlers) : base(handlers) { }

        protected int RenderChildren(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            int maxOrder = order;
            foreach (var child in Children.OfType<IUiElement>())
                maxOrder = Math.Max(maxOrder, child.Render(extents, order + 1, addFunc));
            return maxOrder + 1;
        }

        public abstract Vector2 GetSize();
        public abstract int Render(Rectangle extents, int order, Action<IRenderable> addFunc);
    }
}