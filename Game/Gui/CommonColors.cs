using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;

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
            BorderTexture = factory.CreateEightBitTexture(
                "CommonColors",
                1, 1, 1, (uint) Palette.Count,
                Palette.OrderBy(x => x.Value).Select(x => (byte) x.Key).ToArray(),
                Palette.OrderBy(x => x.Value)
                    .Select(x => new SubImage(Vector2.Zero, Vector2.One, Vector2.One, x.Value))
                    .ToArray());
        }
    }
}
