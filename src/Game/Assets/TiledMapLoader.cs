using System;
using System.Globalization;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Exporters.Tiled;

namespace UAlbion.Game.Assets
{
    public class TiledMapLoader : Component, IAssetLoader<BaseMapData>
    {
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((BaseMapData) existing, info, mapping, s);

        public BaseMapData Serdes(BaseMapData existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (s == null) throw new ArgumentNullException(nameof(s));

            if (s.IsWriting())
            {
                byte[] bytes = existing switch
                {
                    MapData2D map2d => Write2D(map2d, info),
                    // MapData3D map3d => Write3D(map3d),
                    _ => null
                };

                if (bytes != null) s.Bytes(null, bytes, bytes.Length);
                return existing;
            }

            throw new NotImplementedException();
        }

        byte[] Write2D(MapData2D map, AssetInfo info)
        {
            var assets = Resolve<IAssetManager>();
            var disk = Resolve<IFileSystem>();

            TilesetData tileset = assets.LoadTileData(map.TilesetId);
            if (tileset == null)
                return null;

            var tilesetPattern = info.Get(AssetProperty.TilesetPattern, "../Tilesets/{0}_{2}.tsx");
            var tilesetPath = string.Format(CultureInfo.InvariantCulture,
                tilesetPattern,
                map.TilesetId.Id,
                0,
                ConfigUtil.AssetName(map.TilesetId));

            var npcTilesetPath = map.MapType == MapType.TwoDOutdoors 
                ? info.Get(AssetProperty.SmallNpcs, "../Tilesets/SmallNPCs.tsx")
                : info.Get(AssetProperty.LargeNpcs, "../Tilesets/LargeNPCs.tsx");

            // Resolve to absolute path
            var config = Resolve<IGeneralConfig>();
            var destPath = config.ResolvePath(info.File.Filename);
            npcTilesetPath = Path.Combine(destPath, npcTilesetPath);

            var npcTileset = Tileset.Load(npcTilesetPath, disk);
            var properties = new TilemapProperties { TileWidth = 16, TileHeight = 16 };
            var formatter = new EventFormatter(assets.LoadString, map.Id.ToMapText());
            var tiledMap = Map.FromAlbionMap(map, tileset, properties, tilesetPath, npcTileset, formatter);

            return FormatUtil.BytesFromTextWriter(tiledMap.Serialize);
        }
/*
        byte[] Write3D(MapData3D map3d)
        {
            return null;
        }
*/
    }
}