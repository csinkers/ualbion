using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class FixedPositionStack : UiElement
    {
        readonly IList<Child> _positions = new List<Child>();

        class Child
        {
            public Child(IUiElement element, int x, int y, int? width, int? height)
            {
                Element = element;
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            public IUiElement Element { get; }
            public int X { get; }
            public int Y { get; }
            public int? Width { get; }
            public int? Height { get; }
        }

        public FixedPositionStack() : base(null)
        {
        }

        public FixedPositionStack Add(IUiElement child, int x, int y)
        {
            _positions.Add(new Child(child, x, y, null, null));
            Children.Add(child);
            return this;
        }

        public FixedPositionStack Add(IUiElement child, int x, int y, int w, int h)
        {
            _positions.Add(new Child(child, x, y, w, h));
            Children.Add(child);
            return this;
        }

        public override Vector2 GetSize()
        {
            if(!_positions.Any())
                return Vector2.Zero;

            var size = Vector2.Zero;
            foreach(var child in _positions)
            {
                var childSize = child.Element.GetSize();
                childSize.X = child.Width ?? childSize.X;
                childSize.Y = child.Height ?? childSize.Y;

                childSize += new Vector2(child.X, child.Y);

                size.X = Math.Max(size.X, childSize.X);
                size.Y = Math.Max(size.Y, childSize.Y);
            }

            return size;
        }

        int DoLayout(Rectangle extents, int order, Func<Rectangle, int, IUiElement, int> func)
        {
            int maxOrder = order;
            foreach (var child in _positions)
            {
                var childSize = child.Width.HasValue && child.Height.HasValue ? Vector2.Zero : child.Element.GetSize(); 
                var childExtents = new Rectangle(
                    extents.X + child.X,
                    extents.Y + child.Y,
                    (int)(child.Width ?? childSize.X),
                    (int)(child.Height ?? childSize.Y));

                maxOrder = Math.Max(maxOrder, func(childExtents, order + 1, child.Element));
            }
            return maxOrder;
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc) 
            => DoLayout(extents, order, (childExtents, childOrder, child) => child.Render(childExtents, childOrder, addFunc));
        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc) 
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return order;

            return DoLayout(extents, order, (childExtents, childOrder, child) => child.Select(uiPosition, childExtents, childOrder, registerHitFunc));
        }
    }
}