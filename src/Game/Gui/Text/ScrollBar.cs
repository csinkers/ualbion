using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Text
{
    public class ScrollBar : UiElement
    {
        readonly Func<(int, int, int)> _getPosition; // (position, totalHeight, pageHeight)
        readonly UiRectangle _rectangle;

        public ScrollBar(CommonColor color, int width, Func<(int, int, int)> getPosition)
        {
            _getPosition = getPosition;
            Width = width;
            _rectangle = AttachChild(new UiRectangle(color));
        }

        public int Width { get; }

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            var (position, totalHeight, pageHeight) = _getPosition();
            _rectangle.MeasureSize = extents.Size;
            _rectangle.DrawSize = new Vector2(Width, extents.Height * (float)pageHeight / totalHeight);
            var rect = new Rectangle(
                extents.X,
                extents.Y + (int)(extents.Height * ((float)position / totalHeight)),
                extents.Width,
                extents.Height);
            return func(_rectangle, rect, order + 1);
        }
    }
}
