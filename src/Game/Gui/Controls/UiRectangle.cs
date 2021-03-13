using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Gui.Controls
{
    public class UiRectangle : UiElement
    {
        CommonColor _color;
        SpriteLease _sprite;
        bool _dirty = true;
        Vector2 _drawSize;
        Vector3 _lastPosition;

        public UiRectangle(CommonColor color)
        {
            On<BackendChangedEvent>(_ => _dirty = true);
            On<WindowResizedEvent>(_ => _dirty = true);
            _color = color;
        }

        public Vector2 DrawSize
        {
            get => _drawSize;
            set
            {
                if (_drawSize == value) return;
                _drawSize = value;
                _dirty = true;
            }
        }

        public Vector2 MeasureSize { get; set; }
        protected override void Unsubscribed()
        {
            _sprite?.Dispose();
            _sprite = null;
        }

        public CommonColor Color
        {
            get => _color;
            set
            {
                if (_color == value)
                    return;

                _color = value;
                _dirty = true;
            }
        }

        public override Vector2 GetSize() => MeasureSize;
        void Rebuild(Vector3 position, DrawLayer order)
        {
            _dirty = false;
            _lastPosition = position;

            var window = Resolve<IWindowManager>();
            var sm = Resolve<ISpriteManager>();
            var commonColors = Resolve<ICommonColors>();

            var key = new SpriteKey(commonColors.BorderTexture, order, SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);
            if (key != _sprite?.Key)
            {
                _sprite?.Dispose();
                _sprite = sm.Borrow(key, 1, this);
            }

            var instances = _sprite.Access();
            var subImage = new SubImage(
                Vector2.Zero,
                Vector2.One,
                Vector2.One,
                (int)commonColors.Palette[_color]);

            instances[0] = SpriteInstanceData.TopLeft(
                position,
                window.UiToNormRelative(DrawSize),
                subImage,
                SpriteFlags.None);
        }

        public override int Render(Rectangle extents, int order)
        {
            if (!IsSubscribed)
                return order;

            var window = Resolve<IWindowManager>();
            var position = new Vector3(window.UiToNorm(extents.X, extents.Y), 0);
            if (_dirty || position != _lastPosition || _sprite?.Key.RenderOrder != (DrawLayer)order)
                Rebuild(position, (DrawLayer)order);

            return order;
        }
    }
}
