using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats;
using UAlbion.Formats.Ids;

namespace UAlbion;

class FrameStats
{
    static void StatsForEnum<T>(IAssetManager assets, Dictionary<AssetId, int> dict) where T : unmanaged, Enum
    {
        foreach (var enumValue in Enum.GetValues(typeof(T)).OfType<T>())
        {
            var id = AssetId.From(enumValue);
            var info = assets.GetAssetInfo(id);
            var texture = assets.LoadTexture(id) as IReadOnlyTexture<byte>;
            if (texture == null)
                continue;

            var palette = assets.LoadPalette(info.PaletteId);

            var frames = texture.Regions.Count;
            var uniqueColours = new HashSet<byte>();
            foreach (var colour in texture.PixelData)
                uniqueColours.Add(colour);

            var lcm = BlitUtil.CalculatePalettePeriod(uniqueColours, palette);
            dict[id] = (int)ApiUtil.Lcm(frames, lcm);
        }
    }

    public static (AssetId id, int frames)[] FindImagePeriods(IAssetManager assets)
    {
        var dict = new Dictionary<AssetId, int>(); // frames, palPeriods, LCM
        StatsForEnum<Base.Floor>(assets, dict);
        StatsForEnum<Base.Wall>(assets, dict);
        StatsForEnum<Base.WallOverlay>(assets, dict);

        return dict
            .Where(x => x.Value > 1)
            .Select(x => (id: x.Key, frames: x.Value))
            .OrderByDescending(x => x.frames)
            .ToArray();
    }
}
