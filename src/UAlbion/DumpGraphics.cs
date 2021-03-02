using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats.Assets;
using UAlbion.Game;

namespace UAlbion
{
    static class DumpGraphics
    {
        public static void Dump(IAssetManager assets, string baseDir, ISet<AssetType> types, DumpFormats formats, AssetId[] dumpIds)
        {
            var hints = PaletteHints.Load(Path.Combine(baseDir, "mods", "Base", "palette_hints.json"));
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

                    ExportImage(assetId, assets, hints, directory, formats, (frame, palFrame) => palFrame < 10); // Limit to 10, some of the tile sets can get a bit silly.
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
                    case AssetType.LargeNpcGraphics:      Export<Base.LargeNpc>         ("NpcLarge");             break;
                    case AssetType.LargePartyGraphics:    Export<Base.LargePartyMember> ("PartyLarge");           break;
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

        public static IList<ExportedImageInfo> ExportImage(
            AssetId assetId,
            IAssetManager assets,
            PaletteHints hints,
            string directory,
            DumpFormats formats,
            Func<int, int, bool> frameFilter = null)
        {
            var filenames = new List<ExportedImageInfo>();
            var config = assets.GetAssetInfo(assetId);
            AlbionPalette palette;
            if (config != null)
            {
                var rawPaletteId = hints.Get(config.File.Filename, config.SubAssetId);
                var paletteId = new PaletteId(AssetType.Palette, rawPaletteId);
                palette = assets.LoadPalette(paletteId);
            }
            else palette = assets.LoadPalette(Base.Palette.Inventory);

            var texture = assets.LoadTexture(assetId);
            if (texture == null)
                return filenames;

            if (texture is TrueColorTexture trueColor)
            {
                var path = Path.Combine(directory, $"{assetId.Id}_{assetId}");
                var image = trueColor.ToImage();
                Save(image, path, formats, filenames);
            }
            else if (texture is VeldridEightBitTexture tilemap && (
                assetId.Type == AssetType.Font ||
                assetId.Type == AssetType.TilesetGraphics ||
                assetId.Type == AssetType.AutomapGraphics))
            {
                if (palette == null)
                {
                    CoreUtil.LogError($"Could not load palette for {assetId}");
                    return filenames;
                }

                var colors = tilemap.DistinctColors(null);
                int palettePeriod = palette.CalculatePeriod(colors);

                for (int palFrame = 0; palFrame < palettePeriod; palFrame++)
                {
                    if (frameFilter != null && !frameFilter(0, palFrame))
                        continue;
                    var path = Path.Combine(directory, $"{assetId.Id}_{palFrame}_{assetId}");
                    var image = tilemap.ToImage(palette.GetPaletteAtTime(palFrame));
                    Save(image, path, formats, filenames);
                }
            }
            else if (texture is VeldridEightBitTexture ebt)
            {
                for (int subId = 0; subId < ebt.SubImageCount; subId++)
                {
                    if (palette == null)
                    {
                        CoreUtil.LogError($"Could not load palette for {assetId}");
                        break;
                    }

                    var colors = ebt.DistinctColors(subId);
                    int palettePeriod = palette.CalculatePeriod(colors);

                    for (int palFrame = 0; palFrame < palettePeriod; palFrame++)
                    {
                        if (frameFilter != null && !frameFilter(subId, palFrame))
                            continue;
                        var path = Path.Combine(directory, $"{assetId.Id}_{subId}_{palFrame}_{assetId}");
                        var image = ebt.ToImage(subId, palette.GetPaletteAtTime(palFrame));
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

            if ((formats & DumpFormats.Tga) != 0)
            {
                var path = Path.ChangeExtension(pathWithoutExtension, "tga");
                using var stream = File.OpenWrite(path);
                image.SaveAsTga(stream);
                filenames.Add(new ExportedImageInfo { Path = path, Format = DumpFormats.Png, Width = image.Width, Height = image.Height });
            }
        }
    }
}
