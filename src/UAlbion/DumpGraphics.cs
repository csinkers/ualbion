using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Game;

namespace UAlbion
{
    static class DumpGraphics
    {
        public static void Dump(IAssetManager assets, string baseDir, ISet<AssetType> types, DumpFormats formats)
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
                    ExportImage(assetId, assets, directory, formats, 10); // Limit to 10, some of the tile sets can get a bit silly.
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
                    case AssetType.BigNpcGraphics:      Export<Base.LargeNpc>         ("NpcLarge");             break;
                    case AssetType.BigPartyGraphics:    Export<Base.LargePartyMember> ("PartyLarge");           break;
                    case AssetType.MonsterGraphics:     Export<Base.MonsterGraphics>  ("Monster");              break;
                    case AssetType.Picture:             Export<Base.Picture>          ("Picture");              break;
                    case AssetType.SmallNpcGraphics:    Export<Base.SmallNpc>         ("NpcSmall");             break;
                    case AssetType.SmallPartyGraphics:  Export<Base.SmallPartyMember> ("PartySmall");           break;
                    case AssetType.Portrait:            Export<Base.Portrait>         ("Portrait");             break;
                    case AssetType.TacticalIcon:        Export<Base.TacticalGraphics> ("TacticalIcon");         break;
                }
            }
        }

        public static IList<string> ExportImage(AssetId assetId, IAssetManager assets, string directory, DumpFormats formats, int? paletteFrameLimit)
        {
            var filenames = new List<string>();
            var config = assets.GetAssetInfo(assetId);
            var palette = assets.LoadPalette((Base.Palette)(config?.PaletteHint ?? (int)Base.Palette.Inventory));
            var texture = assets.LoadTexture(assetId);
            if (texture == null)
                return null;

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
                    return null;
                }

                var colors = tilemap.DistinctColors(null);
                int palettePeriod = palette.CalculatePeriod(colors);
                if (paletteFrameLimit.HasValue && palettePeriod > paletteFrameLimit)
                    palettePeriod = paletteFrameLimit.Value;

                for (int palFrame = 0; palFrame < palettePeriod; palFrame++)
                {
                    var path = Path.Combine(directory, $"{assetId.Id}_{palFrame}_{assetId}");
                    var image = tilemap.ToImage(null, palette.GetPaletteAtTime(palFrame));
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
                    if (paletteFrameLimit.HasValue && palettePeriod > paletteFrameLimit)
                        palettePeriod = paletteFrameLimit.Value;

                    for (int palFrame = 0; palFrame < palettePeriod; palFrame++)
                    {
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

        static void Save(Image<Rgba32> image, string pathWithoutExtension, DumpFormats formats, IList<string> filenames)
        {
            if ((formats & DumpFormats.Png) != 0)
            {
                var path = Path.ChangeExtension(pathWithoutExtension, "png");
                using var stream = File.OpenWrite(path);
                image.SaveAsPng(stream);
                filenames.Add(path);
            }

            if ((formats & DumpFormats.Tga) != 0)
            {
                var path = Path.ChangeExtension(pathWithoutExtension, "tga");
                using var stream = File.OpenWrite(path);
                image.SaveAsTga(stream);
                filenames.Add(path);
            }
        }
    }
}
