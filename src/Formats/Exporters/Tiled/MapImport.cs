using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled
{
    public static class MapImport
    {
        public static BaseMapData ToAlbion(this Map map, AssetInfo info)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (info == null) throw new ArgumentNullException(nameof(info));

            // Check width/height <= 255
            // Check layer existence

            var rawUnderlay = map.Layers.First(x => x.Name == "Underlay").Data.Content;
            var rawOverlay = map.Layers.First(x => x.Name == "Overlay").Data.Content;

            var underlay = ParseCsv(rawUnderlay).ToArray();
            var overlay = ParseCsv(rawOverlay).ToArray();


            // Check underlay/overlay length matches W*H

            /*
                ObjectGroups = new[] {
                    BuildTriggers(map, properties, eventFormatter, ref nextObjectGroupId, ref nextObjectId),
                    BuildNpcs(map, properties, eventFormatter, npcTileset, ref nextObjectGroupId, ref nextObjectId)
                }.SelectMany(x => x).ToList()
            */

            var result = new MapData2D(info.AssetId, (byte)map.Width, (byte)map.Height)
            {
                Events = { },
                Chains = { },
                Zones = { },
                RawLayout = FormatUtil.ToPacked(underlay, overlay, 1)
            };
            // Post-processing, unswizzle event nodes, build zone lookups etc
            return result;
        }

        static IEnumerable<int> ParseCsv(string csv)
        {
            if (string.IsNullOrEmpty(csv))
                yield break;

            int n = 0;
            foreach (var c in csv)
            {
                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        n *= 10;
                        n += c - '0';
                        break;
                    case ',':
                        yield return n;
                        n = 0;
                        break;
                }
            }
            yield return n;
        }
    }
}