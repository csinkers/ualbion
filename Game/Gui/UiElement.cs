using System;
using System.Linq;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui
{
    /// <summary>
    /// Abstract base class providing reasonable defaults for the methods
    /// on IUiElement. By default the size will be that of the largest
    /// child element, and when calculating layout the full extents
    /// will be passed through to each child.
    /// </summary>
    public abstract class UiElement : Component, IUiElement
    {
        protected Vector2 GetMaxChildSize()
        {
            Vector2 size = Vector2.Zero;
            if (Children != null)
            {
                foreach (var child in Children.OfType<IUiElement>().Where(x => x.IsActive))
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

        protected virtual int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            int maxOrder = order;
            foreach (var child in Children.OfType<IUiElement>().Where(x => x.IsActive))
                maxOrder = Math.Max(maxOrder, func(child, extents, order + 1));
            return maxOrder;
        }

        public virtual Vector2 GetSize() => GetMaxChildSize();
        public virtual int Render(Rectangle extents, int order) => DoLayout(extents, order,
            (child,childExtents,childOrder) => child.Render(childExtents, childOrder));

        public virtual int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return order;

            var maxOrder = DoLayout(extents, order, (x,y,z) => x.Select(uiPosition, y, z, registerHitFunc));
            registerHitFunc(order, this);
            return maxOrder;
        }

        public virtual int Layout(Rectangle extents, int order, LayoutNode parent)
        {
            var node = new LayoutNode(parent, this, extents, order);
            return DoLayout(extents, order,
                (child,childExtents,childOrder) => child.Layout(childExtents, childOrder, node));
        }
    }
}
