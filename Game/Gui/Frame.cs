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

    public class Frame : Component, IUiElement
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
                    void Set(CoreSpriteId textureId, int bias)
                    {
                        multi.AddTexture(
                            1,
                            assets.LoadTexture(textureId),
                            (uint)(TileSize*i + bias),
                            (uint)(TileSize*j + bias),
                            0,
                            true);
                    }

                    void SetLine(bool vertical, int bias)
                    {
                        int modulo = vertical ? j % 4 : i % 4;
                        var texture = assets.LoadTexture(CoreSpriteId.UiBackgroundLines1 + modulo);
                        if (vertical)
                            texture = Core.Util.BuildRotatedTexture((EightBitTexture)texture);

                        multi.AddTexture(
                            1,
                            texture,
                            (uint)(TileSize*i + (vertical ? bias : 0)),
                            (uint)(TileSize*j + (vertical ? 0 : bias)),
                            0,
                            true);
                    }

                    /*
                    if(i % 2 == 0 && j % 4 == 0) // Background is 32x64 compared to 16x16 for the corners
                        Set(CoreSpriteId.UiBackground, 6);
                    */

                    if (j == 0)
                    {
                        if (i == 0)               Set(CoreSpriteId.UiWindowTopLeft, 0);
                        else if (i == tilesW - 1) Set(CoreSpriteId.UiWindowTopRight, 0);
                        else                      SetLine(false, 4);
                    }
                    else if (j == tilesH - 1)
                    {
                        if (i == 0)               Set(CoreSpriteId.UiWindowBottomLeft, 0);
                        else if (i == tilesW - 1) Set(CoreSpriteId.UiWindowBottomRight, 0);
                        else                      SetLine(false, 9);
                    }
                    else
                    {
                        if (i == 0)               SetLine(true, 4);
                        else if (i == tilesW - 1) SetLine(true, 9);
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

        public Vector2 GetSize() => Children.OfType<IUiElement>().Max(x => x.GetSize()) + Vector2.One * TileSize * 2;

        public void Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            Rebuild(extents.Width, extents.Height, order);

            var window = Exchange.Resolve<IWindowManager>();
            _sprite.Position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            addFunc(_sprite);
            var innerExtents = new Rectangle(
                extents.X + TileSize,
                 extents.Y + TileSize,
                extents.Width - TileSize * 2,
                extents.Height - TileSize * 2);

            foreach (var child in Children.OfType<IUiElement>())
                child.Render(innerExtents, order + 1, addFunc);
        }
    }
}
