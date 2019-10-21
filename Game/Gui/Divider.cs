using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class Divider : UiElement
    {
        readonly ITexture _pixel;

        public Divider(CommonColor color) : base(null)
        {
            byte paletteColor = (byte)color;

            _pixel = new EightBitTexture(
                "Divider",
                1, 1, 1, 1,
                new[] { paletteColor },
                new[] { new EightBitTexture.SubImage(0, 0, 1, 1, 0), });
        }
        public override Vector2 GetSize() => new Vector2(0, 1);

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            var window = Resolve<IWindowManager>();
            var size = window.UiToNormRelative(new Vector2(extents.Width, extents.Height));
            var flags = SpriteFlags.NoTransform | SpriteFlags.UsePalette | SpriteFlags.LeftAligned | SpriteFlags.NoDepthTest;

            // TODO: Cache sprite and rebuild when necessary
            var instances = new []
            {
                new SpriteInstanceData(
                    Vector3.Zero,
                    size,
                    Vector2.Zero,
                    Vector2.One,
                    0,
                    flags
                )
            };

            addFunc(new UiMultiSprite(new SpriteKey(_pixel, order, flags))
            {
                Position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0),
                Instances = instances,
            });
            return order;
        }
    }
}
