using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class Frame : Component, IUiElement
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<Frame, WindowResizedEvent>((x, _) => x.Rebuild()),
            new Handler<Frame, SetLanguageEvent>((x, _) => x.Rebuild()), // Make this bubble up from the Text elements?
        };

        MultiSprite _sprite;

        public Frame(IUiElement child) : base(Handlers) => Children.Add(child);
        protected override void Subscribed() => Rebuild();

        void Rebuild()
        {
            var extents = GetSize();
            int width = (int)extents.X;
            int height = (int)extents.Y;

            var assets = Exchange.Resolve<IAssetManager>();
            var multi = new MultiTexture("MainMenu", assets.LoadPalette(PaletteId.Main3D).GetCompletePalette());
            var background = assets.LoadTexture(CoreSpriteId.UiBackground);
            multi.AddTexture(1, background, 6, 6, 0, true, (uint)width - 6, (uint)height - 6);
            int tilesW = (width + 14) / 16;
            int tilesH = (height + 14) / 16;
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
                    else if (j == height - 1)
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

            var window = Exchange.Resolve<IWindowManager>();
            multi.GetSubImageDetails(multi.GetSubImageAtTime(1, 0), out var size, out var offset, out var texSize, out var layer);
            var normalisedSize = window.UiToNormRelative(new Vector2(size.X, size.Y));
            _sprite =
                new MultiSprite(new SpriteKey(multi, (int)DrawLayer.Interface, false))
                {
                    Instances = new[] { new SpriteInstanceData(
                        Vector3.Zero,
                        normalisedSize,
                    offset, texSize, layer, SpriteFlags.NoTransform)
                    },
                    Flags = SpriteFlags.LeftAligned
                };
        }

        public Vector2 GetSize() => 
            Children.OfType<IUiElement>().Max(x => x.GetSize()) + Vector2.One * 32;

        public void Render(Rectangle extents, Action<IRenderable> addFunc)
        {
            var window = Exchange.Resolve<IWindowManager>();
            _sprite.Position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            addFunc(_sprite);
            var innerExtents = new Rectangle(
                extents.X + 16,
                 extents.Y + 16,
                extents.Width - 32,
                extents.Height - 32);

            foreach (var child in Children.OfType<IUiElement>())
                child.Render(innerExtents, addFunc);
        }
    }
}
