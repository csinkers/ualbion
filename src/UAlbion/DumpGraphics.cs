using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion;

class DumpGraphics : Component, IAssetDumper
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
            var assets = Resolve<IAssetManager>();
            foreach (var id in ids)
            {
                var assetId = AssetId.From(id);
                if (dumpIds != null && !dumpIds.Contains(assetId))
                    continue;

                ExportImage(assetId, assets, directory, _formats, (frame, palFrame) => palFrame < 10); // Limit to 10, some of the tile sets can get a bit silly.
            }
        }

        foreach (var type in types)
        {
            switch (type)
            {
                // case AssetType.Slab:                Export<Base.Slab>             ("SLAB");                 break;
                case AssetType.AutomapGraphics:     Export<Base.AutomapTiles>     ("Automap");              break;
                case AssetType.CombatBackground:    Export<Base.CombatBackground> ("CombatBackgrounds");    break;
                case AssetType.CombatGraphics:      Export<Base.CombatGraphics>   ("Combat");               break;
                case AssetType.CoreGraphics:        Export<Base.CoreSprite>       ("Core");                 break;
                case AssetType.BackgroundGraphics:  Export<Base.DungeonBackground>("Backgrounds");          break;
                case AssetType.Floor:               Export<Base.Floor>            ("Floors");               break;
                case AssetType.Object3D:            Export<Base.DungeonObject>    ("Objects");              break;
                case AssetType.WallOverlay:         Export<Base.WallOverlay>      ("Overlays");             break;
                case AssetType.Wall:                Export<Base.Wall>             ("Walls");                break;
                case AssetType.Font:                Export<Base.Font>             ("Fonts");                break;
                case AssetType.FullBodyPicture:     Export<Base.FullBodyPicture>  ("InventoryBackgrounds"); break;
                case AssetType.TilesetGraphics:     Export<Base.TilesetGraphics>  ("Tiles");                break;
                case AssetType.ItemGraphics:        Export<Base.ItemGraphics>     ("Item");                 break;
                case AssetType.LargeNpcGraphics:    Export<Base.LargeNpc>         ("NpcLarge");             break;
                case AssetType.LargePartyGraphics:  Export<Base.LargePartyMember> ("PartyLarge");           break;
                case AssetType.MonsterGraphics:     Export<Base.MonsterGraphics>  ("Monster");              break;
                case AssetType.Picture:             Export<Base.Picture>          ("Picture");              break;
                case AssetType.SmallNpcGraphics:    Export<Base.SmallNpc>         ("NpcSmall");             break;
                case AssetType.SmallPartyGraphics:  Export<Base.SmallPartyMember> ("PartySmall");           break;
                case AssetType.Portrait:            Export<Base.Portrait>         ("Portrait");             break;
                case AssetType.TacticalIcon:        Export<Base.TacticalGraphics> ("TacticalIcon");         break;
            }
        }
    }

    public class ExportedImageInfo
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
            var rawPaletteId = config.Get(AssetProperty.PaletteId, 0);
            var paletteId = new PaletteId(AssetType.Palette, rawPaletteId);
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
        else if (texture is IReadOnlyTexture<byte> tilemap && (
                     assetId.Type == AssetType.Font ||
                     assetId.Type == AssetType.TilesetGraphics ||
                     assetId.Type == AssetType.AutomapGraphics))
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

    static void Save(Image<Rgba32> image, string pathWithoutExtension, DumpFormats formats, IList<ExportedImageInfo> filenames)
    {
        if ((formats & DumpFormats.Png) != 0)
        {
            var path = Path.ChangeExtension(pathWithoutExtension, "png");
            using var stream = File.OpenWrite(path);
            image.SaveAsPng(stream);
            filenames.Add(new ExportedImageInfo { Path = path, Format = DumpFormats.Png, Width = image.Width, Height = image.Height });
        }
    }
}