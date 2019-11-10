using System.Numerics;

namespace UAlbion.Game.Gui
{
    public class FixedSize : UiElement, IFixedSizeUiElement
    {
        readonly int _width;
        readonly int _height;

        public FixedSize(int width, int height, IUiElement child) : base(null)
        {
            _width = width;
            _height = height;
            Children.Add(child);
        }

        public override Vector2 GetSize() => new Vector2(_width, _height);
    }
}