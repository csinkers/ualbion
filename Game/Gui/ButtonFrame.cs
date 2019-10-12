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
    class ButtonFrame : UiElement
    {
        static readonly ITexture _borderTexture = new EightBitTexture(
            "ButtonBorder",
            1, 1, 1, 2,
            new[]
            {
                (byte)CommonColor.Grey15,
                (byte)CommonColor.Grey8,
                (byte)CommonColor.Black2
            },
            new[]
            {
                new EightBitTexture.SubImage(0, 0, 1, 1, 0),
                new EightBitTexture.SubImage(0, 0, 1, 1, 1),
                new EightBitTexture.SubImage(0, 0, 1, 1, 2),
            });

        static readonly Handler[] Handlers = { };

        public ButtonFrame(IUiElement child) : base(Handlers)
        {
            if (child != null)
                Children.Add(child);
        }

        public override Vector2 GetSize() => GetMaxChildSize() + 4 * Vector2.One;

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            var window = Exchange.Resolve<IWindowManager>();
            // TODO: Cache sprite and rebuild when necessary
            var instances = new[]
            {
                new SpriteInstanceData( // Top
                    Vector3.Zero,
                    window.UiToNormRelative(new Vector2(extents.Width - 1, 1)),
                    Vector2.Zero,
                    Vector2.One,
                    0,
                    SpriteFlags.NoTransform | SpriteFlags.UsePalette | SpriteFlags.Transparent
                ),
                new SpriteInstanceData( // Bottom
                    new Vector3(window.UiToNormRelative(new Vector2(1, extents.Height - 1)), 0),
                    window.UiToNormRelative(new Vector2(extents.Width - 1, 1)),
                    Vector2.Zero,
                    Vector2.One,
                    2,
                    SpriteFlags.NoTransform | SpriteFlags.UsePalette | SpriteFlags.Transparent
                ),
                new SpriteInstanceData( // Left
                    new Vector3(window.UiToNormRelative(new Vector2(0, 1)), 0),
                    window.UiToNormRelative(new Vector2(1, extents.Height - 2)),
                    Vector2.Zero,
                    Vector2.One,
                    0,
                    SpriteFlags.NoTransform | SpriteFlags.UsePalette | SpriteFlags.Transparent
                ),
                new SpriteInstanceData( // Right
                    new Vector3(window.UiToNormRelative(new Vector2(extents.Width - 1, 1)), 0),
                    window.UiToNormRelative(new Vector2(1, extents.Height - 2)),
                    Vector2.Zero,
                    Vector2.One,
                    2,
                    SpriteFlags.NoTransform | SpriteFlags.UsePalette | SpriteFlags.Transparent
                ),

                new SpriteInstanceData( // Bottom Left Corner
                    new Vector3(window.UiToNormRelative(new Vector2(0, extents.Height - 1)), 0),
                    window.UiToNormRelative(Vector2.One),
                    Vector2.Zero,
                    Vector2.One,
                    1,
                    SpriteFlags.NoTransform | SpriteFlags.UsePalette | SpriteFlags.Transparent
                ),
                new SpriteInstanceData( // Top Right Corner
                    new Vector3(window.UiToNormRelative(new Vector2(extents.Width - 1, 0)), 0),
                    window.UiToNormRelative(Vector2.One),
                    Vector2.Zero,
                    Vector2.One,
                    1,
                    SpriteFlags.NoTransform | SpriteFlags.UsePalette | SpriteFlags.Transparent
                ),
            };

            addFunc(new UiMultiSprite(new SpriteKey(_borderTexture, order, false))
            {
                Position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0),
                Instances = instances,
                Flags = SpriteFlags.LeftAligned
            });

            var innerExtents = new Rectangle(extents.X + 2, extents.Y + 2, extents.Width - 4, extents.Height - 4);
            return RenderChildren(innerExtents, order, addFunc);
        }
    }
}