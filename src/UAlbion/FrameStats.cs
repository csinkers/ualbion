using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.Assets;
using UAlbion.Game;

namespace UAlbion
{
    class FrameStats
    {
        static void StatsForEnum<T>(IAssetManager assets, Dictionary<AssetId, int> dict) where T : unmanaged, Enum
        {
            foreach (var enumValue in Enum.GetValues(typeof(T)).OfType<T>())
            {
                var id = AssetId.From(enumValue);
                var info = assets.GetAssetInfo(id);
                var texture = assets.LoadTexture(id) as EightBitTexture;
                if (texture == null)
                    continue;

                var palleteId = new PaletteId(AssetType.Palette, info.Get(AssetProperty.PaletteId, 0));
                var palette = assets.LoadPalette(palleteId);

                var frames = texture.SubImageCount;
                var uniqueColours = texture.PixelData.ToHashSet();
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
}