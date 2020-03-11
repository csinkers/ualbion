using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui
{
    public class FixedPositionStack : UiElement, IFixedSizeUiElement
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
            public override string ToString() => $"{Element} @ <{X}, {Y}>";
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

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
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

                maxOrder = Math.Max(maxOrder, func(child.Element, childExtents, order + 1));
            }
            return maxOrder;
        }
        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            // The fixed positions may be outside the regular UI area, so don't clip to the extents that are passed in.
            var maxOrder = DoLayout(extents, order, (x,y,z) => x.Select(uiPosition, y, z, registerHitFunc));
            registerHitFunc(order, this);
            return maxOrder;
        }
    }
}
