using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class AssetManager : Component, IAssetManager, IDisposable
    {
        readonly AssetLocator _assetLocator;

        public AssetManager() : base(null) => _assetLocator = AttachChild(new AssetLocator());
        public void Dispose() { _assetLocator.Dispose(); }
        public MapData2D LoadMap2D(MapDataId id) => _assetLocator.LoadAssetCached(AssetType.MapData, id) as MapData2D;
        public MapData3D LoadMap3D(MapDataId id) => _assetLocator.LoadAssetCached(AssetType.MapData, id) as MapData3D;
        public ItemData LoadItem(ItemId id)
        {
            var data = (IList<ItemData>)_assetLocator.LoadAssetCached(AssetType.ItemList, 0);
            if (data[0].Names == null)
            {
                var names = (IList<string>)_assetLocator.LoadAssetCached(AssetType.ItemNames, 0);
                for (int i = 0; i < data.Count; i++)
                    data[i].Names = names.Skip(i * 3).Take(3).ToArray();
            }

            if ((int)id >= data.Count)
                return null;

            return data[(int)id];
        }

        public AlbionPalette LoadPalette(PaletteId id)
        {
            var palette = (AlbionPalette)_assetLocator.LoadAssetCached(AssetType.Palette, id);
            if (palette == null)
                return null;

            var commonPalette = (byte[])_assetLocator.LoadAssetCached(AssetType.PaletteNull, 0);
            palette.SetCommonPalette(commonPalette);

            return palette;
        }

        public ITexture LoadTexture(AssetType type, int id) => type switch
            {
                AssetType.AutomapGraphics    => LoadTexture((AutoMapId)id),
                AssetType.BackgroundGraphics => LoadTexture((DungeonBackgroundId)id),
                AssetType.BigNpcGraphics     => LoadTexture((LargeNpcId)id),
                AssetType.BigPartyGraphics   => LoadTexture((LargePartyGraphicsId)id),
                AssetType.CombatBackground   => LoadTexture((CombatBackgroundId)id),
                AssetType.CombatGraphics     => LoadTexture((CombatGraphicsId)id),
                AssetType.Floor3D            => LoadTexture((DungeonFloorId)id),
                AssetType.Font               => LoadTexture((FontId)id),
                AssetType.FullBodyPicture    => LoadTexture((FullBodyPictureId)id),
                AssetType.IconGraphics       => LoadTexture((IconGraphicsId)id),
                AssetType.ItemGraphics       => LoadTexture((ItemSpriteId)id),
                AssetType.MonsterGraphics    => LoadTexture((MonsterGraphicsId)id),
                AssetType.Object3D           => LoadTexture((DungeonObjectId)id),
                AssetType.Overlay3D          => LoadTexture((DungeonOverlayId)id),
                AssetType.Picture            => LoadTexture((PictureId)id),
                AssetType.SmallNpcGraphics   => LoadTexture((SmallNpcId)id),
                AssetType.SmallPartyGraphics => LoadTexture((SmallPartyGraphicsId)id),
                AssetType.SmallPortrait      => LoadTexture((SmallPortraitId)id),
                AssetType.TacticalIcon       => LoadTexture((TacticId)id),
                AssetType.Wall3D             => LoadTexture((DungeonWallId)id),
                AssetType.CoreGraphics       => LoadTexture((CoreSpriteId)id),
                AssetType.Slab               => (ITexture)_assetLocator.LoadAssetCached(AssetType.Slab, 0),
                _ => (ITexture)_assetLocator.LoadAssetCached(type, id)
            };

        public ITexture LoadTexture<T>(T id)
        {
            if (id is AutoMapId autoMapId) return LoadTexture(autoMapId);
            if (id is CombatBackgroundId combatBackgroundId)     return LoadTexture(combatBackgroundId);
            if (id is CombatGraphicsId combatGraphicsId)         return LoadTexture(combatGraphicsId);
            if (id is CoreSpriteId coreSpriteId)                 return LoadTexture(coreSpriteId);
            if (id is DungeonBackgroundId dungeonBackgroundId)   return LoadTexture(dungeonBackgroundId);
            if (id is DungeonFloorId dungeonFloorId)             return LoadTexture(dungeonFloorId);
            if (id is DungeonObjectId dungeonObjectId)           return LoadTexture(dungeonObjectId);
            if (id is DungeonOverlayId dungeonOverlayeId)        return LoadTexture(dungeonOverlayeId);
            if (id is DungeonWallId dungeonWallId)               return LoadTexture(dungeonWallId);
            if (id is FontId fontId)                             return LoadTexture(fontId);
            if (id is FullBodyPictureId fullBodyPictureId)       return LoadTexture(fullBodyPictureId);
            if (id is IconGraphicsId iconGraphicsId)             return LoadTexture(iconGraphicsId);
            if (id is ItemSpriteId itemSpriteId)                 return LoadTexture(itemSpriteId);
            if (id is LargeNpcId largeNpcId)                     return LoadTexture(largeNpcId);
            if (id is LargePartyGraphicsId largePartyGraphicsId) return LoadTexture(largePartyGraphicsId);
            if (id is MetaFontId metaFontId)                     return LoadTexture(metaFontId);
            if (id is MonsterGraphicsId monsterGraphicsId)       return LoadTexture(monsterGraphicsId);
            if (id is PictureId pictureId)                       return LoadTexture(pictureId);
            if (id is SmallNpcId smallNpcId)                     return LoadTexture(smallNpcId);
            if (id is SmallPartyGraphicsId smallPartyGraphicsId) return LoadTexture(smallPartyGraphicsId);
            if (id is SmallPortraitId smallPortraitId)           return LoadTexture(smallPortraitId);
            if (id is TacticId tacticId)                         return LoadTexture(tacticId);
            if (id is SlabId _) return (ITexture)_assetLocator.LoadAssetCached(AssetType.Slab, 0);
            throw new NotImplementedException();
        }

        public ITexture LoadTexture(AutoMapId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.AutomapGraphics, id);
        public ITexture LoadTexture(CombatBackgroundId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.CombatBackground, id);
        public ITexture LoadTexture(CombatGraphicsId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.CombatGraphics, id);
        public ITexture LoadTexture(CoreSpriteId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.CoreGraphics, id);
        public ITexture LoadTexture(DungeonBackgroundId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.BackgroundGraphics, id);
        public ITexture LoadTexture(DungeonFloorId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.Floor3D, id);
        public ITexture LoadTexture(DungeonObjectId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.Object3D, id);
        public ITexture LoadTexture(DungeonOverlayId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.Overlay3D, id);
        public ITexture LoadTexture(DungeonWallId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.Wall3D, id);
        public ITexture LoadTexture(FontId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.Font, id);
        public ITexture LoadTexture(FullBodyPictureId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.FullBodyPicture, id);
        public ITexture LoadTexture(IconGraphicsId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.IconGraphics, id);
        public ITexture LoadTexture(ItemSpriteId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.ItemGraphics, 0);
        public ITexture LoadTexture(LargeNpcId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.BigNpcGraphics, id);
        public ITexture LoadTexture(LargePartyGraphicsId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.BigPartyGraphics, id);
        public ITexture LoadTexture(MetaFontId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.MetaFont, id);
        public ITexture LoadTexture(MonsterGraphicsId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.MonsterGraphics, id);
        public ITexture LoadTexture(PictureId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.Picture, id);
        public ITexture LoadTexture(SmallNpcId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.SmallNpcGraphics, id);
        public ITexture LoadTexture(SmallPartyGraphicsId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.SmallPartyGraphics, id);
        public ITexture LoadTexture(SmallPortraitId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.SmallPortrait, id);
        public ITexture LoadTexture(TacticId id) => (ITexture)_assetLocator.LoadAssetCached(AssetType.TacticalIcon, id);
        public TilesetData LoadTileData(IconDataId id) => (TilesetData)_assetLocator.LoadAssetCached(AssetType.IconData, id);
        public LabyrinthData LoadLabyrinthData(LabyrinthDataId id) => (LabyrinthData)_assetLocator.LoadAssetCached(AssetType.LabData, id);
        public ITexture LoadFont(FontColor color, bool isBold) => LoadTexture(new MetaFontId(isBold, color));
        public CoreSpriteConfig.BinaryResource LoadCoreSpriteInfo(CoreSpriteId id) =>
            (CoreSpriteConfig.BinaryResource)_assetLocator.LoadAssetCached(AssetType.CoreGraphicsMetadata, id);

        public string LoadString(StringId id, GameLanguage language)
        {
            var stringTable = (IDictionary<int, string>)_assetLocator.LoadAssetCached(id.Type, id.Id, language);
            if (stringTable == null)
                return $"!MISSING STRING-TABLE {id.Type}:{id.Id}:{id.SubId}:{language}!";

            return stringTable.TryGetValue(id.SubId, out var value) 
                ? value 
                : $"!MISSING STRING {id.Type}:{id.Id}:{id.SubId}:{language}!";
        }

        public string LoadString(SystemTextId id, GameLanguage language) => LoadString(new StringId(AssetType.SystemText, 0, (int)id), language);
        public string LoadString(WordId id, GameLanguage language) => LoadString(new StringId(AssetType.Dictionary, (int)id / 500, (int)id), language);

        public AlbionSample LoadSample(AssetType type, int id) => (AlbionSample)_assetLocator.LoadAssetCached(type, id);
        public AlbionVideo LoadVideo(VideoId id, GameLanguage language) => (AlbionVideo)_assetLocator.LoadAssetCached(AssetType.Flic, (int)id, language);
        public CharacterSheet LoadCharacter(AssetType type, PartyCharacterId id) => (CharacterSheet)_assetLocator.LoadAssetCached(type, id);
        public CharacterSheet LoadCharacter(AssetType type, NpcCharacterId id) => (CharacterSheet)_assetLocator.LoadAssetCached(type, id);
        public CharacterSheet LoadCharacter(AssetType type, MonsterCharacterId id) => (CharacterSheet)_assetLocator.LoadAssetCached(type, id);
        public Chest LoadChest(ChestId id) => (Chest)_assetLocator.LoadAssetCached(AssetType.ChestData, id);
        public Chest LoadMerchant(MerchantId id)=> (Chest)_assetLocator.LoadAssetCached(AssetType.MerchantData, id);
        public WordId? ParseWord(string word)
        {
            var words = // Inefficient code, if it ends up being a problem then we can build a reverse dictionary and cache it.
                new[]
                {   // Load the english files as all languages use english {WORDxxx} tags
                    (IDictionary<int, string>) _assetLocator.LoadAssetCached(AssetType.Dictionary, 0),
                    (IDictionary<int, string>) _assetLocator.LoadAssetCached(AssetType.Dictionary, 1),
                    (IDictionary<int, string>) _assetLocator.LoadAssetCached(AssetType.Dictionary, 2)
                };
            return words.SelectMany(x => x).Where(x => x.Value == word).Select(x => (WordId?)x.Key).FirstOrDefault();
        }

        public IList<Block> LoadBlockList(BlockListId blockListId) => (IList<Block>)_assetLocator.LoadAssetCached(AssetType.BlockList, blockListId);
    }
}
