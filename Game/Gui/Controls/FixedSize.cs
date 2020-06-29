using System;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui.Controls
{
    public class FixedSize : UiElement, IFixedSizeUiElement
    {
        readonly int _width;
        readonly int _height;

        public FixedSize(int width, int height, IUiElement child)
        {
            _width = width;
            _height = height;
            Children.Add(child);
        }

        public DialogPositioning Position { get; set; } = DialogPositioning.Center;

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            int shiftX = Math.Max(0, extents.Width - _width);
            int shiftY = Math.Max(0, extents.Height - _height);

            (shiftX, shiftY) = Position switch
            {
                DialogPositioning.Center      => (shiftX/2, shiftY/2),
                DialogPositioning.Top         => (shiftX/2, 0),
                DialogPositioning.Left        => (0, shiftY),
                DialogPositioning.Right       => (shiftX, shiftY),
                DialogPositioning.BottomLeft  => (0, shiftY),
                DialogPositioning.TopLeft     => (0, 0),
                DialogPositioning.TopRight    => (shiftX, 0),
                DialogPositioning.BottomRight => (shiftX, shiftY),
                DialogPositioning.Bottom      => (shiftX/2, shiftY),
                _ => (shiftX / 2, shiftY / 2),
            };

            var fixedExtents = new Rectangle(extents.X + shiftX, extents.Y + shiftY, _width, _height);
            return base.DoLayout(fixedExtents, order, func);
        }

        public override Vector2 GetSize() => new Vector2(_width, _height);
        public override string ToString() => $"FixedSize: <{_width}, {_height}> {Position} {Children[0]}";
    }
}
