using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Gui
{
    public class CommonColors : ICommonColors
    {
        public IDictionary<CommonColor, uint> Palette { get; } =
            Enum.GetValues(typeof(CommonColor))
                .Cast<CommonColor>()
                .Select((x, i) => (x, i))
                .ToDictionary(x => x.x, x => (uint)x.i);

        public ITexture BorderTexture { get; }

        public CommonColors(ICoreFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            BorderTexture = factory.CreateEightBitTexture(
                AssetId.None,
                "CommonColors",
                1, 1, 1, Palette.Count,
                Palette.OrderBy(x => x.Value).Select(x => (byte)x.Key).ToArray(),
                Palette.OrderBy(x => x.Value)
                    .Select(x => new SubImage(Vector2.Zero, Vector2.One, Vector2.One, (int)x.Value))
                    .ToArray());
        }
    }
}
