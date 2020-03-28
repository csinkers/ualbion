using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui.Controls
{
    public class Divider : UiElement
    {
        readonly CommonColor _color;
        SpriteLease _sprite;
        Vector3 _lastPosition;
        Vector2 _lastSize;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<Divider, ExchangeDisabledEvent>((x, _) => { x._sprite?.Dispose(); x._sprite = null; }));

        public Divider(CommonColor color) : base(Handlers)
        {
            _color = color;
        }
        public override Vector2 GetSize() => new Vector2(0, 1);

        void UpdateSprite(Vector3 position, Vector2 size, DrawLayer layer)
        {
            var commonColors = Resolve<ICommonColors>();
            if(layer != _sprite?.Key.RenderOrder)
            {
                _sprite?.Dispose();

                var sm = Resolve<ISpriteManager>();
                var key = new SpriteKey(commonColors.BorderTexture, layer, SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest);
                _sprite = sm.Borrow(key, 1, this);
            }
            else if (_lastPosition == position && _lastSize == size)
                return;

            var instances = _sprite.Access();
            instances[0] = SpriteInstanceData.TopLeft(position, size, _sprite, (int)commonColors.Palette[_color], 0);
            _lastPosition = position;
            _lastSize = size;
        }

        public override int Render(Rectangle extents, int order)
        {
            var window = Resolve<IWindowManager>();
            var size = window.UiToNormRelative(new Vector2(extents.Width, extents.Height));
            var position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            UpdateSprite(position, size, (DrawLayer)order);
            return order;
        }
    }
}
