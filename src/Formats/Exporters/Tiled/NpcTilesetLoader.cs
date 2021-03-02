using System;
using System.Collections.Generic;
using System.Globalization;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class NpcTilesetLoader : IAssetLoader
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s == null) throw new ArgumentNullException(nameof(s));

            if (s.IsWriting())
            {
                if (existing == null) throw new ArgumentNullException(nameof(existing));
                var graphicsPattern = config.Get(AssetProperty.GraphicsPattern, "");
                bool small = config.Get(AssetProperty.IsSmall, false);

                var tiles = new List<Tiled.TileProperties>();
                var assetIds = AssetMapping.Global.EnumerateAssetsOfType(small ? AssetType.SmallNpcGraphics : AssetType.LargeNpcGraphics);
                foreach (var id in assetIds)
                {
                    var path = string.Format(CultureInfo.InvariantCulture,
                        graphicsPattern,
                        id.Id, 9, // 9 = First frame facing west for both large and small
                        ConfigUtil.AssetName(id));

                    tiles.Add(new TileProperties
                    {
                        Name = id.ToString(),
                        Frames = 1,
                        Source = path
                    });
                }

                var tilemap = Tileset.FromSprites(small ? "SmallNPCs" : "LargeNPCs", "NPC", tiles);
                var bytes = FormatUtil.BytesFromTextWriter(tilemap.Serialize);
                s.ByteArray(null, bytes, bytes.Length);

                return existing;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}