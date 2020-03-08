using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;

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

        protected UiElement() : this(null) { }
        protected UiElement(IDictionary<Type, Handler> handlers) : base(handlers) { }

        protected virtual int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            int maxOrder = order;
            foreach (var child in Children.OfType<IUiElement>())
                maxOrder = Math.Max(maxOrder, func(child, extents, order + 1));
            return maxOrder;
        }

        public virtual Vector2 GetSize() => GetMaxChildSize();
        public virtual int Render(Rectangle extents, int order) => DoLayout(extents, order, (x,y,z) => x.Render(y, z));
        public virtual int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return order;

            var maxOrder = DoLayout(extents, order, (x,y,z) => x.Select(uiPosition, y, z, registerHitFunc));
            registerHitFunc(order, this);
            return maxOrder;
        }
    }
}