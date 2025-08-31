using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Gui;

public class CommonColors : ICommonColors
{
    public IDictionary<CommonColor, uint> Palette { get; } =
        Enum.GetValues<CommonColor>()
            .Select((x, i) => (x, i))
            .ToDictionary(x => x.x, x => (uint)x.i);

    public IReadOnlyTexture<byte> BorderTexture { get; }
    public Region GetRegion(CommonColor color) => BorderTexture.Regions[(int)Palette[color]];

    public CommonColors()
    {
        var texture = new ArrayTexture<byte>(
            AssetId.None,
            "CommonColors",
            1, 1, Palette.Count,
            Palette.OrderBy(x => x.Value).Select(x => (byte)x.Key).ToArray());

        foreach (var entry in Palette.OrderBy(x => x.Value))
            texture.AddRegion(0, 0, 1, 1, (int)entry.Value);

        BorderTexture = texture;
    }
}