using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Core.Textures;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game;

namespace UAlbion
{
    static class DumpGraphics
    {
        // For now, ignore formats and just dump out PNG.
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

            void Export<TEnum>(string name, Func<TEnum, AssetKey> keyFunc, Func<TEnum, ITexture> loadFunc)
            {
                var directory = Path.Combine(baseDir, "data", "exported", "gfx", name);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var ids = Enum.GetValues(typeof(TEnum)).OfType<TEnum>().ToArray();
                Console.WriteLine($"Dumping {ids.Length} assets to {directory}...");
                foreach (var id in ids)
                {
                    int intId = Convert.ToInt32(id, CultureInfo.InvariantCulture);
                    var config = assets.GetAssetInfo(keyFunc(id)) as FullAssetInfo;
                    var palette = assets.LoadPalette((PaletteId)(config?.PaletteHints?.FirstOrDefault() ?? (int)PaletteId.Inventory));
                    var texture = loadFunc(id);
                    if (texture == null)
                        continue;

                    if (texture is TrueColorTexture trueColor)
                    {
                        var path = Path.Combine(directory, $"{intId}_{id}");
                        var image = trueColor.ToImage();
                        Save(image, path);
                    }
                    else if (texture is VeldridEightBitTexture tilemap && (
                        typeof(TEnum) == typeof(FontId) ||
                        typeof(TEnum) == typeof(IconGraphicsId) ||
                        typeof(TEnum) == typeof(AutoGraphicsId)))
                    {
                        var colors = tilemap.DistinctColors(null);
                        int palettePeriod = palette.CalculatePeriod(colors);
                        for (int palFrame = 0; palFrame < palettePeriod; palFrame++)
                        {
                            var path = Path.Combine(directory, $"{intId}_{palFrame}_{id}");
                            var image = tilemap.ToImage(null, palette.GetPaletteAtTime(palFrame));
                            Save(image, path);
                        }
                    }
                    else if (texture is VeldridEightBitTexture ebt)
                    {
                        for (int subId = 0; subId < ebt.SubImageCount; subId++)
                        {
                            if (id is ItemSpriteId)
                                subId = Convert.ToInt32(id, CultureInfo.InvariantCulture);

                            var colors = ebt.DistinctColors(subId);
                            int palettePeriod = palette.CalculatePeriod(colors);
                            for (int palFrame = 0; palFrame < palettePeriod; palFrame++)
                            {
                                var path = Path.Combine(directory, $"{intId}_{subId}_{palFrame}_{id}");
                                var image = ebt.ToImage(subId, palette.GetPaletteAtTime(palFrame));
                                Save(image, path);
                            }

                            if (id is ItemSpriteId)
                                break;
                        }
                    }
                    else
                    {
                        var path = Path.Combine(directory, $"{intId}_{id}.png.todo");
                        File.WriteAllText(path, "");
                    }
                }
            }

            foreach (var type in types)
            {
                switch (type)
                {
                    case AssetType.Slab:                Export<SlabId>              ("SLAB",                 x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.AutomapGraphics:     Export<AutoGraphicsId>      ("Automap",              x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.CombatBackground:    Export<CombatBackgroundId>  ("CombatBackgrounds",    x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.CombatGraphics:      Export<CombatGraphicsId>    ("Combat",               x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.CoreGraphics:        Export<CoreSpriteId>        ("Core",                 x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.BackgroundGraphics:  Export<DungeonBackgroundId> ("Backgrounds",          x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.Floor3D:             Export<DungeonFloorId>      ("Floors",               x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.Object3D:            Export<DungeonObjectId>     ("Objects",              x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.Overlay3D:           Export<DungeonOverlayId>    ("Overlays",             x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.Wall3D:              Export<DungeonWallId>       ("Walls",                x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.Font:                Export<FontId>              ("Fonts",                x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.FullBodyPicture:     Export<FullBodyPictureId>   ("InventoryBackgrounds", x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.IconGraphics:        Export<IconGraphicsId>      ("Tiles",                x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.ItemGraphics:        Export<ItemSpriteId>        ("Item",                 x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.BigNpcGraphics:      Export<LargeNpcId>          ("NpcLarge",             x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.BigPartyGraphics:    Export<LargePartyGraphicsId>("PartyLarge",           x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.MonsterGraphics:     Export<MonsterGraphicsId>   ("Monster",              x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.Picture:             Export<PictureId>           ("Picture",              x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.SmallNpcGraphics:    Export<SmallNpcId>          ("NpcSmall",             x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.SmallPartyGraphics:  Export<SmallPartyGraphicsId>("PartySmall",           x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.SmallPortrait:       Export<SmallPortraitId>     ("Portrait",             x => (AssetId)x, assets.LoadTexture); break;
                    case AssetType.TacticalIcon:        Export<TacticId>            ("TacticalIcon",         x => (AssetId)x, assets.LoadTexture); break;
                }
            }
        }
    }
}
