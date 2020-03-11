using System;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui
{
    public class Padding : UiElement
    {
        readonly int _left;
        readonly int _right;
        readonly int _top;
        readonly int _bottom;

        public Padding(IUiElement content, int allAround)
        {
            _left = _right = _top = _bottom = allAround;
            AttachChild(content);
        }

        public Padding(IUiElement content, int horizontal, int vertical)
        {
            _left = _right = horizontal;
            _top = _bottom = vertical;
            AttachChild(content);
        }

        public Padding(IUiElement content, int top, int right, int bottom, int left)
        {
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
            AttachChild(content);
        }

        public override Vector2 GetSize()
        {
            var contentSize =  base.GetSize();
            return new Vector2(contentSize.X + _left + _right, contentSize.Y + _top + _bottom);
        }

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            var paddedExtents = new Rectangle(
                extents.X + _left,
                extents.Y + _top,
                extents.Width - _left - _right,
                extents.Height - _top - _bottom
            );
            return base.DoLayout(paddedExtents, order, func);
        }
    }
}
