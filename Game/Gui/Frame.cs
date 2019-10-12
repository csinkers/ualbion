using System;
using System.Linq;
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
        const int TileSize = 16;
        MultiSprite _sprite;
        Vector2 _lastPixelSize;

        public Frame(IUiElement child) : base(null) => Children.Add(child);
        void Rebuild(int width, int height, int order)
        {
            var window = Exchange.Resolve<IWindowManager>();

            {
                var normSize = window.UiToNormRelative(new Vector2(width, height));
                var pixelSize = window.NormToPixelRelative(normSize);

                if ((pixelSize - _lastPixelSize).LengthSquared() < float.Epsilon && _sprite.RenderOrder == order)
                    return;
                _lastPixelSize = pixelSize;
            }

            var assets = Exchange.Resolve<IAssetManager>();
            var multi = new MultiTexture("MainMenu", assets.LoadPalette(PaletteId.Main3D).GetCompletePalette());
            var background = assets.LoadTexture(CoreSpriteId.UiBackground);
            multi.AddTexture(1, background, 16, 16, 0, true, (uint)width - 32, (uint)height - 32);
            int tilesW = width / TileSize;
            int tilesH = height / TileSize;
            for (int j = 0; j < tilesH; j++)
            {
                for (int i = 0; i < tilesW; i++)
                {
                    void Set(CoreSpriteId textureId, int offsetx, int offsety)
                    {
                        multi.AddTexture(
                            1,
                            assets.LoadTexture(textureId),
                            (uint)(TileSize*i + offsetx),
                            (uint)(TileSize*j + offsety),
                            0,
                            true);
                    }

                    void SetLine(bool vertical, int offsetx, int offsety)
                    {
                        int modulo = vertical ? j % 4 : i % 4;
                        var texture = assets.LoadTexture(CoreSpriteId.UiBackgroundLines1 + modulo);
                        if (vertical)
                            texture = Core.Util.BuildRotatedTexture((EightBitTexture)texture);

                        multi.AddTexture(
                            1,
                            texture,
                            (uint)(TileSize*i + offsetx),
                            (uint)(TileSize*j + offsety),
                            0,
                            true);
                    }

                    /*
                    if(i % 2 == 0 && j % 4 == 0) // Background is 32x64 compared to 16x16 for the corners
                        Set(CoreSpriteId.UiBackground, 6);
                    */

                    if (j == 0) // Top
                    {
                        if (i == 0)               Set(CoreSpriteId.UiWindowTopLeft, 9, 9);
                        else if (i == tilesW - 1) Set(CoreSpriteId.UiWindowTopRight, 3, 9);
                        else                      SetLine(false, 9, 13);
                    }
                    else if (j == tilesH - 1) // Bottom
                    {
                        if (i == 0)               Set(CoreSpriteId.UiWindowBottomLeft, 9, -6);
                        else if (i == tilesW - 1) Set(CoreSpriteId.UiWindowBottomRight, 3, -6);
                        else                      SetLine(false, 9, 3);
                    }
                    else
                    {
                        if (i == 0)               SetLine(true, 13, 9); // Left edge
                        else if (i == tilesW - 1) SetLine(true, 12, 9); // Right edge
                    }
                }
            }

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

        public override Vector2 GetSize() => Children.OfType<IUiElement>().Max(x => x.GetSize()) + Vector2.One * TileSize * 2;

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            Rebuild(extents.Width, extents.Height, order);

            var window = Exchange.Resolve<IWindowManager>();
            var innerExtents = new Rectangle(
                extents.X + TileSize,
                 extents.Y + TileSize,
                extents.Width - TileSize * 2,
                extents.Height - TileSize * 2);

            _sprite.Position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            _sprite.RenderOrder = order; // Render the frame in front of its children
            addFunc(_sprite);

            return RenderChildren(innerExtents, order, addFunc);
        }
    }
}
