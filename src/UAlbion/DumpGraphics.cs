using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game;

namespace UAlbion;

sealed class DumpGraphics : GameComponent, IAssetDumper
{
    readonly DumpFormats _formats;

    public DumpGraphics(DumpFormats formats) => _formats = formats;

    public void Dump(string baseDir, ISet<AssetType> types, AssetId[] dumpIds)
    {
        void Export<TEnum>(string name) where TEnum : unmanaged, Enum
        {
            var directory = Path.Combine(baseDir, "data", "exported", "gfx", name);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var ids = Enum.GetValues(typeof(TEnum)).OfType<TEnum>().ToArray();
            Console.WriteLine($"Dumping {ids.Length} assets to {directory}...");
            foreach (var id in ids)
            {
                var assetId = AssetId.From(id);
                if (dumpIds != null && !dumpIds.Contains(assetId))
                    continue;

                ExportImage(assetId, Assets, directory, _formats, (_, palFrame) => palFrame < 10); // Limit to 10, some of the tile sets can get a bit silly.
            }
        }

        foreach (var type in types)
        {
            switch (type)
            {
                // case AssetType.Slab:                Export<Base.Slab>             ("SLAB");                 break;
                case AssetType.AutomapGfx:        Export<Base.AutomapTiles>     ("Automap");              break;
                case AssetType.CombatBackground:  Export<Base.CombatBackground> ("CombatBackgrounds");    break;
                case AssetType.CombatGfx:         Export<Base.CombatGfx>        ("Combat");               break;
                case AssetType.CoreGfx:           Export<Base.CoreGfx>          ("Core");                 break;
                case AssetType.BackgroundGfx:     Export<Base.DungeonBackground>("Backgrounds");          break;
                case AssetType.Floor:             Export<Base.Floor>            ("Floors");               break;
                case AssetType.Object3D:          Export<Base.DungeonObject>    ("Objects");              break;
                case AssetType.WallOverlay:       Export<Base.WallOverlay>      ("Overlays");             break;
                case AssetType.Wall:              Export<Base.Wall>             ("Walls");                break;
                case AssetType.FontGfx:           Export<Base.Font>             ("Fonts");                break;
                case AssetType.PartyInventoryGfx: Export<Base.PartyInventoryGfx>("InventoryBackgrounds"); break;
                case AssetType.TilesetGfx:        Export<Base.TilesetGfx>       ("Tiles");                break;
                case AssetType.ItemGfx:           Export<Base.ItemGfx>          ("Item");                 break;
                case AssetType.NpcLargeGfx:       Export<Base.NpcLargeGfx>      ("NpcLarge");             break;
                case AssetType.PartyLargeGfx:     Export<Base.PartyLargeGfx>    ("PartyLarge");           break;
                case AssetType.MonsterGfx:        Export<Base.MonsterGfx>       ("Monster");              break;
                case AssetType.Picture:           Export<Base.Picture>          ("Picture");              break;
                case AssetType.NpcSmallGfx:       Export<Base.NpcSmallGfx>      ("NpcSmall");             break;
                case AssetType.PartySmallGfx:     Export<Base.PartySmallGfx>    ("PartySmall");           break;
                case AssetType.Portrait:          Export<Base.Portrait>         ("Portrait");             break;
                case AssetType.TacticalGfx:       Export<Base.TacticalGfx>      ("TacticalIcon");         break;
            }
        }
    }

    public sealed class ExportedImageInfo
    {
        public string Path { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public DumpFormats Format { get; set; }
    }

    public IList<ExportedImageInfo> ExportImage(
        AssetId assetId,
        IAssetManager assets,
        string directory,
        DumpFormats formats,
        Func<int, int, bool> frameFilter = null)
    {
        var filenames = new List<ExportedImageInfo>();
        var config = assets.GetAssetInfo(assetId);
        AlbionPalette palette;
        if (config != null)
        {
            var rawPaletteId = config.GetProperty(AssetProps.Palette);
            var paletteId = new PaletteId(rawPaletteId);
            palette = assets.LoadPalette(paletteId);
        }
        else palette = assets.LoadPalette(Base.Palette.Inventory);

        var texture = assets.LoadTexture(assetId);
        if (texture == null)
            return filenames;

        if (texture is IReadOnlyTexture<uint> trueColor)
        {
            var path = Path.Combine(directory, $"{assetId.Id}_{assetId}");
            var image = ImageSharpUtil.ToImageSharp(trueColor.GetLayerBuffer(0));
            Save(image, path, formats, filenames);
        }
        else if (texture is IReadOnlyTexture<byte> tilemap && assetId.Type is AssetType.FontGfx or AssetType.TilesetGfx or AssetType.AutomapGfx)
        {
            if (palette == null)
            {
                Error($"Could not load palette for {assetId}");
                return filenames;
            }

            var colors = BlitUtil.DistinctColors(tilemap.PixelData);
            int palettePeriod = palette.CalculatePeriod(colors);

            for (int palFrame = 0; palFrame < palettePeriod; palFrame++)
            {
                if (frameFilter != null && !frameFilter(0, palFrame))
                    continue;
                var path = Path.Combine(directory, $"{assetId.Id}_{palFrame}_{assetId}");
                var image = ImageSharpUtil.ToImageSharp(tilemap.GetLayerBuffer(0), palette.GetPaletteAtTime(palFrame));
                Save(image, path, formats, filenames);
            }
        }
        else if (texture is IReadOnlyTexture<byte> eightBit)
        {
            for (int subId = 0; subId < eightBit.Regions.Count; subId++)
            {
                if (palette == null)
                {
                    Error($"Could not load palette for {assetId}");
                    break;
                }

                var colors = BlitUtil.DistinctColors(eightBit.GetRegionBuffer(subId));
                int palettePeriod = palette.CalculatePeriod(colors);

                for (int palFrame = 0; palFrame < palettePeriod; palFrame++)
                {
                    if (frameFilter != null && !frameFilter(subId, palFrame))
                        continue;
                    var path = Path.Combine(directory, $"{assetId.Id}_{subId}_{palFrame}_{assetId}");
                    var image = ImageSharpUtil.ToImageSharp(eightBit.GetRegionBuffer(subId), palette.GetPaletteAtTime(palFrame));
                    Save(image, path, formats, filenames);
                }
            }
        }
        else
        {
            var path = Path.Combine(directory, $"{assetId.Id}_{assetId}.png.todo");
            File.WriteAllText(path, "");
            return null;
        }

        return filenames;
    }

    static void Save(Image<Rgba32> image, string pathWithoutExtension, DumpFormats formats, List<ExportedImageInfo> filenames)
    {
        if ((formats & DumpFormats.Png) == 0)
            return;

        var path = Path.ChangeExtension(pathWithoutExtension, "png");
        using var stream = File.OpenWrite(path);
        image.SaveAsPng(stream);
        filenames.Add(new ExportedImageInfo { Path = path, Format = DumpFormats.Png, Width = image.Width, Height = image.Height });
    }
}
