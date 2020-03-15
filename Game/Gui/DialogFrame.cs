using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui
{
    public class DialogFrame : UiElement
    {
        const int TileSize = 16;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<DialogFrame, ExchangeDisabledEvent>((x, _) => { x._sprite?.Dispose(); x._sprite = null; })
        );

        PositionedSpriteBatch _sprite;
        Vector2 _lastPixelSize; // For dirty state detection

        public DialogFrame(IUiElement child) : base(Handlers) => Children.Add(child);
        public DialogFrameBackgroundStyle Background { get; set; }
        void Rebuild(int width, int height, DrawLayer order)
        {
            var shadowSubImage = new SubImage(Vector2.Zero, Vector2.Zero, Vector2.One, 0);
            var window = Resolve<IWindowManager>();
            var sm = Resolve<ISpriteManager>();
            var factory = Resolve<ICoreFactory>();

            { // Check if we need to rebuild
                var normSize = window.UiToNormRelative(new Vector2(width, height));
                var pixelSize = window.NormToPixelRelative(normSize);

                if ((pixelSize - _lastPixelSize).LengthSquared() < float.Epsilon && _sprite?.RenderOrder == order)
                    return;
                _lastPixelSize = pixelSize;
            }

            var assets = Resolve<IAssetManager>();
            var multi = factory.CreateMultiTexture($"DialogFrame {width}x{height}", new DummyPaletteManager(assets.LoadPalette(PaletteId.Inventory)));

            void DrawLine(uint y)
            {
                uint x = 16;
                uint n = 0;
                while(x < width - TileSize)
                {
                    var texture = assets.LoadTexture((CoreSpriteId)((int)CoreSpriteId.UiBackgroundLines1 + n % 4));
                    uint? w = x + 2*TileSize > width ? (uint)(width - TileSize - x) : (uint?)null;
                    multi.AddTexture(1, texture, x, y, 0, true, w);
                    n++;
                    x += TileSize;
                }
            }

            void DrawVerticalLine(uint x)
            {
                uint y = 16;
                uint n = 0;
                var texture = assets.LoadTexture((CoreSpriteId)((int)CoreSpriteId.UiBackgroundLines1 + n % 4));
                texture = CoreUtil.BuildRotatedTexture(factory, (EightBitTexture)texture);
                while (y < height - TileSize)
                {
                    uint? h = y + 2*TileSize > height ? (uint)(height - TileSize - y) : (uint?)null;
                    multi.AddTexture(1, texture, x, y, 0, true, null, h);
                    n++;
                    y += TileSize;
                }
            }

            // Background
            switch (Background)
            {
                case DialogFrameBackgroundStyle.MainMenuPattern:
                {
                    var background = assets.LoadTexture(CoreSpriteId.UiBackground);
                    multi.AddTexture(1, background, 7, 7, 0, true, (uint) width - 14, (uint) height - 14);
                    break;
                }

                case DialogFrameBackgroundStyle.DarkTint:
                {
                    var colors = Resolve<ICommonColors>();
                    multi.AddTexture(1, colors.BorderTexture, 7, 7, 0, false, (uint)width - 14, (uint)height - 14, 128);
                    break;
                }
            }

            // Corners
            multi.AddTexture(1, assets.LoadTexture(CoreSpriteId.UiWindowTopLeft), 0, 0, 0, true);
            multi.AddTexture(1, assets.LoadTexture(CoreSpriteId.UiWindowTopRight), (uint)width - 16, 0, 0, true);
            multi.AddTexture(1, assets.LoadTexture(CoreSpriteId.UiWindowBottomLeft), 0, (uint)height - 16, 0, true);
            multi.AddTexture(1, assets.LoadTexture(CoreSpriteId.UiWindowBottomRight), (uint)width - 16, (uint)height - 16, 0, true);

            DrawLine(4); // Left
            DrawLine((uint)height - 7); // Right
            DrawVerticalLine(4); // Top
            DrawVerticalLine((uint)width - 7); // Bottom

            var subImage = multi.GetSubImageDetails(multi.GetSubImageAtTime(1, 0));
            var normalisedSize = window.UiToNormRelative(subImage.Size);

            var key = new SpriteKey(multi, order, SpriteKeyFlags.NoTransform);
            _sprite?.Dispose();

            var lease = sm.Borrow(key, 2, this);
            var flags = SpriteFlags.None.SetOpacity(0.5f);
            var instances = lease.Access();

            var shadowPosition = new Vector3(window.UiToNormRelative(new Vector2(10, 10)), 0);
            var shadowSize = window.UiToNormRelative(subImage.Size - new Vector2(10, 10));
            instances[0] = SpriteInstanceData.TopLeft(shadowPosition, shadowSize, shadowSubImage, flags);// Drop shadow
            instances[1] = SpriteInstanceData.TopLeft(Vector3.Zero, normalisedSize, subImage, 0); // DialogFrame
            _sprite = new PositionedSpriteBatch(lease, normalisedSize);
        }

        public override Vector2 GetSize() => GetMaxChildSize() + Vector2.One * 14;

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            var innerExtents = new Rectangle(
                extents.X + 7,
                 extents.Y + 7,
                extents.Width - 14,
                extents.Height - 14);

            return base.DoLayout(innerExtents, order, func);
        }

        public override int Render(Rectangle extents, int order)
        {
            Rebuild(extents.Width, extents.Height, (DrawLayer)order);

            var window = Resolve<IWindowManager>();
            _sprite.Position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);

            return base.Render(extents, order);
        }

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            Rebuild(extents.Width, extents.Height, (DrawLayer)order);
            return base.Select(uiPosition, extents, order, registerHitFunc);
        }
    }
}
