using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game;
using Tiled = UAlbion.Formats.Exporters.Tiled;

namespace UAlbion
{
    public static class DumpTiled
    {
        class DumpProperties
        {
            public DumpProperties(IAssetManager assets, string tilesetDir, string exportDir, Func<string, TextWriter> getWriter)
            {
                Assets = assets;
                TilesetDir = tilesetDir;
                ExportDir = exportDir;
                GetWriter = getWriter;
            }

            public IAssetManager Assets { get; }
            public string TilesetDir { get; }
            public string ExportDir { get; }
            public Func<string, TextWriter> GetWriter { get; }
        }

        public static void Dump(string baseDir, IAssetManager assets, ISet<AssetType> types, AssetId[] dumpIds)
        {
            if (assets == null) throw new ArgumentNullException(nameof(assets));
            if (types == null) throw new ArgumentNullException(nameof(types));
            var disposeList = new List<IDisposable>();
            var exportDir = Path.Combine(baseDir, "data", "exported", "tiled");
            if (!Directory.Exists(exportDir))
                Directory.CreateDirectory(exportDir);

            var tilesetDir = Path.Combine(exportDir, "tilesets");
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

            var props = new DumpProperties(assets, tilesetDir, exportDir, Writer);

            void Flush()
            {
                foreach (var d in disposeList)
                    d.Dispose();
                disposeList.Clear();
            }

            if (types.Contains(AssetType.SmallNpcGraphics))
                DumpNpcTileset(props, "smallnpc", "SmallNPCs", AssetId.EnumerateAll(AssetType.SmallNpcGraphics));
            Flush();

            if (types.Contains(AssetType.BigNpcGraphics))
                DumpNpcTileset(props, "largenpc", "LargeNPCs", AssetId.EnumerateAll(AssetType.BigNpcGraphics));
            Flush();

            if (types.Contains(AssetType.Object3D))
            {
            }

            if (types.Contains(AssetType.Floor))
            {
            }

            if (types.Contains(AssetType.Wall))
            {
            }

            if (types.Contains(AssetType.WallOverlay))
            {
            }

            if (types.Contains(AssetType.AutomapGraphics))
            {
            }

            if (types.Contains(AssetType.TilesetData))
            {
                foreach (TilesetId id in DumpUtil.All(AssetType.TilesetData, dumpIds))
                    Dump2DTilemap(assets, id, props);

                Flush();
            }

            if (types.Contains(AssetType.Map))
            {
                foreach (var id in DumpUtil.All(AssetType.Map, dumpIds))
                {
                    if (assets.LoadMap(id) is MapData2D map2d)
                        Dump2DMap(map2d, assets, props);
                    //if (assets.LoadMap(id) is MapData3D map3d)
                    //    Dump3DMap(map3d, assets, props);
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

        static void DumpNpcTileset(DumpProperties props, string gfxDirName, string tilesetName, IEnumerable<AssetId> assetIds)
        {
            var spriteDir = Path.Combine(props.TilesetDir, gfxDirName);
            if (!Directory.Exists(spriteDir))
                Directory.CreateDirectory(spriteDir);

            var tiles = new List<Tiled.TileProperties>();
            foreach (var id in assetIds)
            {
                var info = DumpGraphics.ExportImage(id, props.Assets, spriteDir, DumpFormats.Png, (frame, palFrame) => frame == 9 && palFrame == 0).SingleOrDefault();
                if (info == null)
                    continue;
                tiles.Add(new Tiled.TileProperties
                {
                    Name = id.ToString(),
                    Frames = 1,
                    Width = info.Width,
                    Height = info.Height,
                    Source = info.Path,
                });
            }

            var tilemap = Tiled.Tileset.FromSprites(tilesetName, "NPC", tiles);
            tilemap.Save(Path.Combine(props.TilesetDir,  tilesetName + ".tsx"));
        }

        static void Dump2DTilemap(IAssetManager assets, in TilesetId id, DumpProperties props)
        {
            var tileGfxDir = Path.Combine(props.TilesetDir, "tiles");
            if (!Directory.Exists(tileGfxDir))
                Directory.CreateDirectory(tileGfxDir);

            TilesetData tileset = assets.LoadTileData(id);
            ITexture sheet = assets.LoadTexture(id.ToTilesetGraphics());
            if (tileset == null) return;
            if (sheet == null) return;

            var sheetInfo = DumpGraphics.ExportImage(
                    id.ToTilesetGraphics(),
                    assets,
                    tileGfxDir,
                    DumpFormats.Png,
                    (frame, palFrame) => frame == 0 && palFrame == 0
                ).FirstOrDefault();

            if (sheetInfo == null)
            {
                CoreUtil.LogError($"Could not save sprite sheet for tilemap {id}");
                return;
            }

            var tw = props.GetWriter(Path.Combine(props.TilesetDir, $"{id.Id}_{id}.tsx"));
            var properties = ExtractProperties(sheet, sheetInfo.Path);
            var tilemap = Tiled.Tileset.FromTileset(tileset, properties);
            tilemap.Serialize(tw);
        }

        static void Dump2DMap(MapData2D map, IAssetManager assets, DumpProperties props)
        {
            TilesetData tileset = assets.LoadTileData(map.TilesetId);
            if (tileset == null)
                return;

            ITexture sheet = assets.LoadTexture(map.TilesetId.ToTilesetGraphics());
            if (sheet == null)
                return;

            var tilesetPath = Path.Combine(props.TilesetDir, $"{map.TilesetId.Id}_{map.TilesetId}.tsx");
            var npcTilesetPath = Path.Combine(props.TilesetDir, map.MapType == MapType.TwoD ? "LargeNPCs.tsx" : "SmallNPCs.tsx");
            var npcTileset = Tiled.Tileset.Load(npcTilesetPath);

            var tw = props.GetWriter(Path.Combine(props.ExportDir, $"{map.Id.Id}_{map.Id}.tmx"));
            var properties = ExtractProperties(sheet, null);
            var formatter = new EventFormatter(assets.LoadString, map.Id.ToMapText());
            var tiledMap = Tiled.Map.FromAlbionMap(map, tileset, properties, tilesetPath, npcTileset, formatter);
            tiledMap.Serialize(tw);
        }

        // static void Dump3DMap(MapData3D map3d, IAssetManager assets, DumpProperties props)
        // {
        // }

        static Tiled.TilemapProperties ExtractProperties(ITexture sheet, string sheetPath) => new Tiled.TilemapProperties
        {
            FrameDurationMs = 180, // TODO: Pull from first matching map's anim rate?
            // Commented out since we export closely packed now instead of dumping the raw in-memory pixels.
            // Margin = 1, // See AlbionSpritePostProcessor
            // Spacing = 2, // See AlbionSpritePostProcessor
            SheetPath = sheetPath,
            SheetWidth = (int)sheet.Width,
            SheetHeight = (int)sheet.Height,
            TileWidth = (int)sheet.GetSubImageDetails(0).Size.X,
            TileHeight = (int)sheet.GetSubImageDetails(0).Size.Y,
        };
    }
}