using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui
{
    public static class CommonColors
    {
        public static readonly IDictionary<CommonColor, uint> Palette = 
            Enum.GetValues(typeof(CommonColor))
                .Cast<CommonColor>()
                .Select((x, i) => (x, i))
                .ToDictionary(x => x.x, x => (uint)x.i);

        public static readonly ITexture BorderTexture  = 
            new EightBitTexture(
                "CommonColors",
                1, 1, 1, (uint)Palette.Count,
                Palette.OrderBy(x => x.Value).Select(x => (byte)x.Key).ToArray(),
                Palette.OrderBy(x => x.Value)
                    .Select(x => new EightBitTexture.SubImage(0, 0, 1, 1, x.Value))
                    .ToArray());
    }
}