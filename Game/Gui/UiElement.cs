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
            return maxOrder;
        }

        protected void SelectChildren(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            foreach (var child in Children.OfType<IUiElement>())
                child.Select(uiPosition, extents, order + 1, registerHitFunc);
        }

        public virtual Vector2 GetSize() => GetMaxChildSize();
        public virtual int Render(Rectangle extents, int order, Action<IRenderable> addFunc) => RenderChildren(extents, order, addFunc);
        public virtual void Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return;

            SelectChildren(uiPosition, extents, order, registerHitFunc);
            registerHitFunc(order, this);
        }
    }
}