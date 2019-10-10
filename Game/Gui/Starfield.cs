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

        public Starfield() : base(null)
        {
            _pixel = new EightBitTexture(
                "Pixel",
                1, 1, 1, 1,
                new byte[] { 1 },
                new[] { new EightBitTexture.SubImage(0, 0, 1, 1, 0), });
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

        public void Render(Rectangle extents, Action<IRenderable> addFunc)
        {
            var window = Exchange.Resolve<IWindowManager>();
            var instances = new List<SpriteInstanceData>();
            for(int j = extents.Y; j < extents.Height; j++)
            {
                for (int i = extents.X; i < extents.Width; i++)
                {
                    int n = instances.Count;
                    SpriteFlags flags = SpriteFlags.NoTransform;
                    if ((n & 1) != 0) flags |= SpriteFlags.BlueTint;
                    if ((n & 2) != 0) flags |= SpriteFlags.GreenTint;
                    if ((n & 4) != 0) flags |= SpriteFlags.RedTint;
                    if ((n & 8) != 0) flags |= SpriteFlags.Highlight;

                    instances.Add(
                        new SpriteInstanceData(
                            new Vector3(window.UiToNorm(new Vector2(i, j)), 0),
                            Vector2.One / window.Size,
                            Vector2.Zero,
                            Vector2.One,
                            0,
                            flags
                        )
                    );
                }
            }

            addFunc(new MultiSprite(new SpriteKey(_pixel, 1, false)) { Instances = instances.ToArray() });
        }
    }
}