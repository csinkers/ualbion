using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Exporters;
using UAlbion.Game;

namespace UAlbion
{
    public static class DumpTiled
    {
        public static void Dump(string baseDir, IAssetManager assets, ISet<AssetType> types)
        {
            if (assets == null) throw new ArgumentNullException(nameof(assets));
            if (types == null) throw new ArgumentNullException(nameof(types));
            var disposeList = new List<IDisposable>();
            string exportDir = Path.Combine(baseDir, "data", "exported", "tiled");
            if (!Directory.Exists(exportDir))
                Directory.CreateDirectory(exportDir);

            string tilesetDir = Path.Combine(exportDir, "tilesets");
            if (!Directory.Exists(tilesetDir))
                Directory.CreateDirectory(tilesetDir);

            TextWriter Writer(string filename)
            {
                var stream = File.Open(filename, FileMode.Create);
                var writer = new StreamWriter(stream);
                disposeList.Add(writer);
                disposeList.Add(stream);
                return writer;
            }

            void Flush()
            {
                foreach (var d in disposeList)
                    d.Dispose();
                disposeList.Clear();
            }

            if (types.Contains(AssetType.TilesetData))
            {
                foreach (TilesetId id in DumpUtil.All(AssetType.TilesetData))
                {
                    TilesetData tileset = assets.LoadTileData(id);
                    ITexture sheet = assets.LoadTexture(id.ToTilesetGraphics());
                    if (tileset == null) continue;
                    if (sheet == null) continue;

                    var sheetPath = DumpGraphics.ExportImage(id.ToTilesetGraphics(), assets, tilesetDir, DumpFormats.Png, 1).FirstOrDefault();
                    if(sheetPath == null)
                    {
                        CoreUtil.LogError($"Could not save sprite sheet for tilemap {id}");
                        continue;
                    }

                    var tw = Writer(Path.Combine(tilesetDir, $"{id.Id}_{id}.tsx"));
                    var properties = ExtractProperties(sheet, sheetPath);
                    var tilemap = TiledTileMap.FromTileset(tileset, properties);
                    tilemap.Serialize(tw);
                }

                Flush();
            }

            if (types.Contains(AssetType.Map))
            {
                foreach (var id in DumpUtil.All(AssetType.Map))
                {
                    MapData2D map = assets.LoadMap(id) as MapData2D;
                    if (map == null) continue;

                    TilesetData tileset = assets.LoadTileData(map.TilesetId);
                    if (tileset == null) continue;

                    ITexture sheet = assets.LoadTexture(map.TilesetId.ToTilesetGraphics());
                    if (sheet == null) continue;

                    var tilesetPath = Path.Combine(tilesetDir, $"{map.TilesetId.Id}_{map.TilesetId}.tsx");
                    var tw = Writer(Path.Combine(exportDir, $"{id.Id}_{id}.tmx"));
                    var properties = ExtractProperties(sheet, null);
                    var tiledMap = TiledMap.FromAlbionMap(map, tileset, properties, tilesetPath);
                    tiledMap.Serialize(tw);
                }

                Flush();
            }

            /* TODO
            if (types.Contains(AssetType.BlockList))
            {
                foreach (var id in DumpUtil.All(AssetType.BlockList))
                {
                    IList<Block> asset = assets.LoadBlockList(id);
                    if (asset == null) continue;
                    tw = Writer($"blocks/blocklist{id.Id}.json");
                    s.Serialize(tw, asset);
                }
                Flush();
            }
            //*/
        }

        static TilemapProperties ExtractProperties(ITexture sheet, string sheetPath) => new TilemapProperties
            {
                FrameDurationMs = 180, // TODO: Pull from first matching map's anim rate?
                Margin = 1, // See AlbionSpritePostProcessor
                Spacing = 2, // See AlbionSpritePostProcessor
                SheetPath = sheetPath,
                SheetWidth = (int)sheet.Width,
                SheetHeight = (int)sheet.Height,
                TileWidth = (int)sheet.GetSubImageDetails(0).Size.X,
                TileHeight = (int)sheet.GetSubImageDetails(0).Size.Y,
            };
    }
}