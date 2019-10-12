using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using Veldrid;

namespace UAlbion.Game.Gui
{
    /*
    public static class GuiHelper
    {
        public static IRenderable HorizontalLine(int x1, int x2, int y, byte color)
        {
        }

        public static IRenderable VerticalLine(int x, int y1, int y2, byte color)
        {
        }

        public static IRenderable HollowRectangle(int x1, int y1, int x2, int y2, byte color1, byte color2)
        {
        }

        public static IRenderable FilledRectangle(int x1, int y1, int x2, int y2, byte color1, byte color2, byte fillColor)
        {
        }
    }*/

    public class Frame : UiElement
    {
        bool _hideCorners = false;
        bool _truncateLines = true;

        const int TileSize = 16;
        MultiSprite _sprite;
        Vector2 _lastPixelSize;

        public Frame(IUiElement child) : base(null) => Children.Add(child);
        void Rebuild(int width, int height, int order)
        {
            var window = Exchange.Resolve<IWindowManager>();

            { // Check if we need to rebuild
                var normSize = window.UiToNormRelative(new Vector2(width, height));
                var pixelSize = window.NormToPixelRelative(normSize);

                if ((pixelSize - _lastPixelSize).LengthSquared() < float.Epsilon && _sprite.RenderOrder == order)
                    return;
                _lastPixelSize = pixelSize;
            }

            var assets = Exchange.Resolve<IAssetManager>();
            var multi = new MultiTexture("MainMenu", assets.LoadPalette(PaletteId.Main3D).GetCompletePalette());

            void DrawLine(uint y)
            {
                uint x = 16;
                uint n = 0;
                while(x < width - TileSize)
                {
                    var texture = assets.LoadTexture((CoreSpriteId)((int)CoreSpriteId.UiBackgroundLines1 + n % 4));
                    uint? w = x + 2*TileSize > width ? (uint)(width - TileSize - x) : (uint?)null;
                    multi.AddTexture(1, texture, x, y, 0, true, w, null);
                    n++;
                    x += TileSize;
                }
            }

            void DrawVerticalLine(uint x)
            {
                uint y = 16;
                uint n = 0;
                var texture = assets.LoadTexture((CoreSpriteId)((int)CoreSpriteId.UiBackgroundLines1 + n % 4));
                texture = Util.BuildRotatedTexture((EightBitTexture)texture);
                while (y < height - TileSize)
                {
                    uint? h = y + 2*TileSize > height ? (uint)(height - TileSize - y) : (uint?)null;
                    multi.AddTexture(1, texture, x, y, 0, true, null, h);
                    n++;
                    y += TileSize;
                }
            }

            // Background
            var background = assets.LoadTexture(CoreSpriteId.UiBackground);
            multi.AddTexture(1, background, 7, 7, 0, true, (uint)width - 14, (uint)height - 14);

            // Corners
            multi.AddTexture(1, assets.LoadTexture(CoreSpriteId.UiWindowTopLeft), 0, 0, 0, true);
            multi.AddTexture(1, assets.LoadTexture(CoreSpriteId.UiWindowTopRight), (uint)width - 16, 0, 0, true);
            multi.AddTexture(1, assets.LoadTexture(CoreSpriteId.UiWindowBottomLeft), 0, (uint)height - 16, 0, true);
            multi.AddTexture(1, assets.LoadTexture(CoreSpriteId.UiWindowBottomRight), (uint)width - 16, (uint)height - 16, 0, true);

            DrawLine(4); // Left
            DrawLine((uint)height - 7); // Right
            DrawVerticalLine(4); // Top
            DrawVerticalLine((uint)width - 7); // Bottom

            multi.GetSubImageDetails(multi.GetSubImageAtTime(1, 0), out var size, out var offset, out var texSize, out var layer);
            var normalisedSize = window.UiToNormRelative(new Vector2(size.X, size.Y));
            _sprite =
                new UiMultiSprite(new SpriteKey(multi, order, false))
                {
                    Instances = new[] { new SpriteInstanceData(
                        Vector3.Zero,
                        normalisedSize,
                    offset, texSize, layer, SpriteFlags.NoTransform)
                    },
                    Flags = SpriteFlags.LeftAligned
                };
        }

        public override Vector2 GetSize() => GetMaxChildSize() + Vector2.One * 14;

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            Rebuild(extents.Width, extents.Height, order);

            var window = Exchange.Resolve<IWindowManager>();
            var innerExtents = new Rectangle(
                extents.X + 7,
                 extents.Y + 7,
                extents.Width - 14,
                extents.Height - 14);

            _sprite.Position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            _sprite.RenderOrder = order; // Render the frame in front of its children
            addFunc(_sprite);

            return RenderChildren(innerExtents, order, addFunc);
        }
    }
}
