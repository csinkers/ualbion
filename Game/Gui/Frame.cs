using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui
{
    public class Frame : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<Frame, SubscribedEvent>((x, _) => x.Rebuild()),
            new Handler<Frame, WindowResizedEvent>((x, _) => x.Rebuild()),
            new Handler<Frame, RenderEvent>((x, e) => x.Render(e)),
        };

        int _x;
        int _y;
        readonly int _width;
        readonly int _height;
        IRenderable[] _sprites;
        public Vector2 Position { get; private set; }

        public Frame(int x, int y, int width, int height) : base(Handlers)
        {
            // Coordinates are for inner / content area.
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        void Render(RenderEvent renderEvent)
        {
            foreach (var sprite in _sprites)
                renderEvent.Add(sprite);
        }

        void Rebuild()
        {
            var assets = Exchange.Resolve<IAssetManager>();
            var sprites = new List<IRenderable>();
            var multi = new MultiTexture("MainMenu", assets.LoadPalette(PaletteId.Main3D).GetCompletePalette());
            var background = assets.LoadTexture(CoreSpriteId.UiBackground);
            multi.AddTexture(1, background, 6, 6, 0, true, (uint)_width + 14, (uint)_height + 14);
            int tilesW = (_width + 14) / 16;
            int tilesH = (_height + 14) / 16;
            for (int j = 0; j < tilesH; j++)
            {
                for (int i = 0; i < tilesW; i++)
                {
                    void Set(CoreSpriteId textureId, int bias)
                    {
                        multi.AddTexture(
                            1,
                            assets.LoadTexture(textureId),
                            (uint)(16*i + bias),
                            (uint)(16*j + bias),
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
                            (uint)(16*i + (vertical ? bias : 0)),
                            (uint)(16*j + (vertical ? 0 : bias)),
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
                    else if (j == _height - 1)
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

            var window = Exchange.Resolve<IWindowState>();
            multi.GetSubImageDetails(multi.GetSubImageAtTime(1, 0), out var size, out var offset, out var texSize, out var layer);
            var normalisedSize = window.UiToScreenRelative((int)size.X, (int)size.Y);
            Position = window.UiToScreen(_x, _y);
            sprites.Add(
                new MultiSprite(new SpriteKey(multi, (int)DrawLayer.Interface, false))
                {
                    Instances = new[] { new SpriteInstanceData(
                        new Vector3(Position, 0),
                        normalisedSize,
                    offset, texSize, layer, SpriteFlags.NoTransform)
                    }
                });
            _sprites = sprites.ToArray();
        }
    }
}
