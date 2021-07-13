using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui.Controls
{
    public class RepeatedBackground : UiElement
    {
        PositionedSpriteBatch _sprite;
        Vector2 _lastPixelSize; // For dirty state detection

        public RepeatedBackground(IUiElement child) => AttachChild(child);
        protected override void Unsubscribed()
        {
            _sprite?.Dispose();
            _sprite = null;
        }

        void Rebuild(int width, int height, DrawLayer order)
        {
            var shadowSubImage = new Region(Vector2.Zero, Vector2.Zero, Vector2.One, 0);
            var window = Resolve<IWindowManager>();
            var sm = Resolve<ISpriteManager>();

            { // Check if we need to rebuild
                var normSize = window.UiToNormRelative(width, height);
                var pixelSize = window.NormToPixelRelative(normSize);

                if ((pixelSize - _lastPixelSize).LengthSquared() < float.Epsilon && _sprite?.RenderOrder == order)
                    return;
                _lastPixelSize = pixelSize;
            }

            var assets = Resolve<IAssetManager>();
            var multi = new CompositedTexture(AssetId.None, $"Background {width}x{height}", assets.LoadPalette(Base.Palette.Inventory));

            // Background
            var background = assets.LoadTexture(Base.CoreSprite.UiBackground);
            multi.AddTexture(1, background, 0, 0, 0, true, width, height);

            var subImage = multi.Regions[multi.GetSubImageAtTime(1, 0, false)];
            var normalisedSize = window.UiToNormRelative(subImage.Size);

            var key = new SpriteKey(multi, SpriteSampler.Point, order, SpriteKeyFlags.NoTransform);
            _sprite?.Dispose();

            var lease = sm.Borrow(key, 2, this);
            var flags = SpriteFlags.None.SetOpacity(0.5f);

            var shadowPosition = new Vector3(window.UiToNormRelative(10, 10), 0);
            var shadowSize = window.UiToNormRelative(subImage.Size - new Vector2(10, 10));

            bool lockWasTaken = false;
            var instances = lease.Lock(ref lockWasTaken);
            try
            {
                instances[0] = new SpriteInstanceData(shadowPosition, shadowSize, shadowSubImage, SpriteFlags.TopLeft | flags); // Drop shadow
                instances[1] = new SpriteInstanceData(Vector3.Zero, normalisedSize, subImage, SpriteFlags.TopLeft); // DialogFrame
            }
            finally { lease.Unlock(lockWasTaken); }

            _sprite = new PositionedSpriteBatch(lease, normalisedSize);
        }

        public override int Render(Rectangle extents, int order)
        {
            Rebuild(extents.Width, extents.Height, (DrawLayer)order);

            var window = Resolve<IWindowManager>();
            _sprite.Position = new Vector3(window.UiToNorm(extents.X, extents.Y), 0);

            return base.Render(extents, order);
        }

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            Rebuild(extents.Width, extents.Height, _sprite?.RenderOrder ?? (DrawLayer)order);
            return base.Select(uiPosition, extents, order, registerHitFunc);
        }
    }
}
