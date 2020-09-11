using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UAlbion.Core.Textures;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game;

namespace UAlbion
{
    static class DumpGraphics
    {
        // For now, ignore formats and just dump out PNG.
        public static void Dump(IAssetManager assets, string baseDir, ISet<AssetType> types, DumpFormats _)
        {
            var factory = new VeldridCoreFactory();
            void Export<TEnum>(string name, Func<TEnum, AssetKey> keyFunc, Func<TEnum, ITexture> loadFunc)
            {
                var directory = Path.Combine(baseDir, "data", "exported", "png", name);
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

                    var path = Path.Combine(directory, $"{intId}_{id}.png");

                    if (texture.ArrayLayers > 1 || texture.SubImageCount > 255)
                    {
                        File.WriteAllText(path + ".todo", "");
                        continue; // TODO: Handle layered images + subimages
                    }

                    if (texture is TrueColorTexture trueColor)
                    {
                        trueColor.SavePng(path);
                    }
                    else
                    {
                        var multiTexture = factory.CreateMultiTexture(name, new DummyPaletteManager(palette));
                        multiTexture.AddTexture(1, texture, 0, 0, null, false);
                        multiTexture.SavePng(1, 0, path);
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
