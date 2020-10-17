using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Config;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Game;

namespace UAlbion
{
    static class DumpGraphics
    {
        public static void Dump(IAssetManager assets, string baseDir, ISet<AssetType> types, DumpFormats formats)
        {
            void Save(Image<Rgba32> image, string pathWithoutExtension)
            {
                if ((formats & DumpFormats.Png) != 0)
                {
                    var path = Path.ChangeExtension(pathWithoutExtension, "png");
                    using var stream = File.OpenWrite(path);
                    image.SaveAsPng(stream);
                }

                if ((formats & DumpFormats.Tga) != 0)
                {
                    var path = Path.ChangeExtension(pathWithoutExtension, "tga");
                    using var stream = File.OpenWrite(path);
                    image.SaveAsTga(stream);
                }
            }

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
                    var config = assets.GetAssetInfo(assetId);
                    var palette = assets.LoadPalette((Base.Palette)(config?.PaletteHints?.FirstOrDefault() ?? (int)Base.Palette.Inventory));
                    var texture = assets.LoadTexture(assetId);
                    if (texture == null)
                        continue;

                    if (texture is TrueColorTexture trueColor)
                    {
                        var path = Path.Combine(directory, $"{assetId.Id}_{id}");
                        var image = trueColor.ToImage();
                        Save(image, path);
                    }
                    else if (texture is VeldridEightBitTexture tilemap && (
                        typeof(TEnum) == typeof(Base.Font) ||
                        typeof(TEnum) == typeof(Base.TilesetGraphics) ||
                        typeof(TEnum) == typeof(Base.AutomapTiles)))
                    {
                        var colors = tilemap.DistinctColors(null);
                        int palettePeriod = palette.CalculatePeriod(colors);
                        for (int palFrame = 0; palFrame < palettePeriod; palFrame++)
                        {
                            var path = Path.Combine(directory, $"{assetId.Id}_{palFrame}_{id}");
                            var image = tilemap.ToImage(null, palette.GetPaletteAtTime(palFrame));
                            Save(image, path);
                        }
                    }
                    else if (texture is VeldridEightBitTexture ebt)
                    {
                        for (int subId = 0; subId < ebt.SubImageCount; subId++)
                        {
                            if (id is Base.ItemGraphics)
                                subId = Convert.ToInt32(id, CultureInfo.InvariantCulture);

                            var colors = ebt.DistinctColors(subId);
                            int palettePeriod = palette.CalculatePeriod(colors);
                            for (int palFrame = 0; palFrame < palettePeriod; palFrame++)
                            {
                                var path = Path.Combine(directory, $"{assetId.Id}_{subId}_{palFrame}_{id}");
                                var image = ebt.ToImage(subId, palette.GetPaletteAtTime(palFrame));
                                Save(image, path);
                            }

                            if (id is Base.ItemGraphics)
                                break;
                        }
                    }
                    else
                    {
                        var path = Path.Combine(directory, $"{assetId.Id}_{id}.png.todo");
                        File.WriteAllText(path, "");
                    }
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
    }
}
