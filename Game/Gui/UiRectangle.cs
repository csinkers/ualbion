using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui
{
    public class UiRectangle : UiElement
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<UiRectangle, WindowResizedEvent>((x, _) => x._dirty = true),
            H<UiRectangle, ExchangeDisabledEvent>((x, _) => { x._sprite?.Dispose(); x._sprite = null; }));

        CommonColor _color;
        SpriteLease _sprite;
        bool _dirty = true;
        Vector2 _drawSize;
        Vector3 _lastPosition;

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

        public UiRectangle(CommonColor color) : base(Handlers)
        {
            _color = color;
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
            instances[0] = SpriteInstanceData.TopLeft(
                position,
                window.UiToNormRelative(DrawSize),
                Vector2.Zero,
                Vector2.One,
                commonColors.Palette[_color],
                SpriteFlags.None);
        }

        public override int Render(Rectangle extents, int order)
        {
            var window = Resolve<IWindowManager>();
            var position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            if (_dirty || position != _lastPosition || _sprite?.Key.RenderOrder != (DrawLayer)order)
                Rebuild(position, (DrawLayer)order);

            return order;
        }
    }
}