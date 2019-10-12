using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class Starfield : Component, IUiElement // UI-grid for testing
    {
        readonly ITexture _pixel;
        MultiSprite _sprite;
        Vector2 _lastPixelSize;


        public Starfield() : base(null)
        {
            _pixel = new EightBitTexture(
                "Pixel",
                1, 1, 1, 1,
                new byte[] { 1 },
                new[] { new EightBitTexture.SubImage(0, 0, 1, 1, 0), });
        }

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

            var totalSize = GetSize();
            var extents = new Rectangle(0, 0, (int)totalSize.X, (int)totalSize.Y);

            var instances = new List<SpriteInstanceData>();
            for (int j = extents.Y; j < extents.Height; j++)
            {
                for (int i = extents.X; i < extents.Width; i++)
                {
                    int n = instances.Count;
                    SpriteFlags flags = SpriteFlags.NoTransform;
                    if ((n & 1) != 0) flags |= SpriteFlags.BlueTint;
                    if ((n & 2) != 0) flags |= SpriteFlags.GreenTint;
                    if ((n & 4) != 0) flags |= SpriteFlags.RedTint;
                    if ((n & 8) != 0) flags |= SpriteFlags.Highlight;

                    var position = new Vector3(window.UiToNorm(new Vector2(i, j)), 0);
                    var size = 2 * Vector2.One / window.Size;
                    instances.Add(
                        new SpriteInstanceData(
                            position,
                            size,
                            Vector2.Zero,
                            Vector2.One,
                            0,
                            flags
                        )
                    );
                }
            }

            _sprite = new MultiSprite(new SpriteKey(_pixel, order, false))
            {
                Instances = instances.ToArray()
            };
        }

        protected override void Subscribed()
        {
            var layout = Exchange.Resolve<ILayoutManager>();
            layout.Add(this, DialogPositioning.Center);
        }

        public Vector2 GetSize()
        {
            var window = Exchange.Resolve<IWindowManager>();
            return new Vector2(window.UiWidth, window.UiHeight);
        }

        public int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            Rebuild(extents.Width, extents.Height, order);
            addFunc(_sprite);
            return order;
        }
    }
}