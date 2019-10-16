using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class SliderTrack : UiElement
    {
        readonly string _id;
        readonly SliderThumb _thumb;
        readonly Func<int> _getter;
        readonly int _min;
        readonly int _max;

        public SliderTrack(string id, Func<int> getter, int min, int max) : base(null)
        {
            _id = id;
            _getter = getter;
            _min = min;
            _max = max;
            _thumb = new SliderThumb(getter);
            Children.Add(_thumb);
        }

        public override Vector2 GetSize() => _thumb.GetSize();
        Rectangle ThumbPosition(Rectangle extents)
        {
            var size = _thumb.GetSize();
            size.X = (int)Math.Max(size.X, (float)extents.Width / (_max - _min));
            int spareWidth = extents.Width - (int)size.X;
            int currentValue = _getter();

            int position = extents.X + (int)(spareWidth * (float)(currentValue - _min) / (_max - _min));
            return new Rectangle(position, extents.Y, (int)size.X, extents.Height);
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            return _thumb.Render(ThumbPosition(extents), order, addFunc);
        }

        public override void Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return;

            _thumb.Select(uiPosition, ThumbPosition(extents), order, registerHitFunc);
        }
    }
}