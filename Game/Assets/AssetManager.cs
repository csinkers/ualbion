﻿using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Map;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class AssetManager : Component, IAssetManager, IDisposable
    {
        readonly AssetLocatorRegistry _assetLocatorRegistry;

        public AssetManager()
        {
            _assetLocatorRegistry = AttachChild(new AssetLocatorRegistry());
        }
        public AssetManager AddAssetLocator(IAssetLocator locator)
        {
            _assetLocatorRegistry.AddAssetLocator(locator);
            return this;
        }

        public AssetManager AddAssetPostProcessor(IAssetPostProcessor postProcessor)
        {
            _assetLocatorRegistry.AddAssetPostProcessor(postProcessor);
            return this;
        }

        public void Dispose() { _assetLocatorRegistry.Dispose(); }
        public IMapData LoadMap(MapDataId id) => (IMapData)_assetLocatorRegistry.LoadAsset(AssetType.MapData, id); // No caching for map data
        public ItemData LoadItem(ItemId id)
        {
            var data = (IList<ItemData>)_assetLocatorRegistry.LoadAssetCached(AssetType.ItemList, 0);
            if (data[0].Names == null)
            {
                var names = (IList<string>)_assetLocatorRegistry.LoadAssetCached(AssetType.ItemNames, 0);
                for (int i = 0; i < data.Count; i++)
                    data[i].Names = names.Skip(i * 3).Take(3).ToArray();
            }

            if ((int)id >= data.Count)
                return null;

            return data[(int)id];
        }

        public AlbionPalette LoadPalette(PaletteId id)
        {
            var palette = (AlbionPalette)_assetLocatorRegistry.LoadAssetCached(AssetType.Palette, id);
            if (palette == null)
                return null;

            var commonPalette = (byte[])_assetLocatorRegistry.LoadAssetCached(AssetType.PaletteNull, 0);
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
                AssetType.Slab               => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.Slab, 0),
                _ => (ITexture)_assetLocatorRegistry.LoadAssetCached(type, id)
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
            if (id is SlabId _) return (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.Slab, 0);
            throw new NotImplementedException();
        }

        public ITexture LoadTexture(AutoMapId id)                  => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.AutomapGraphics, id);
        public ITexture LoadTexture(CombatBackgroundId id)         => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.CombatBackground, id);
        public ITexture LoadTexture(CombatGraphicsId id)           => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.CombatGraphics, id);
        public ITexture LoadTexture(CoreSpriteId id)               => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.CoreGraphics, id);
        public ITexture LoadTexture(DungeonBackgroundId id)        => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.BackgroundGraphics, id);
        public ITexture LoadTexture(DungeonFloorId id)             => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.Floor3D, id);
        public ITexture LoadTexture(DungeonObjectId id)            => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.Object3D, id);
        public ITexture LoadTexture(DungeonOverlayId id)           => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.Overlay3D, id);
        public ITexture LoadTexture(DungeonWallId id)              => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.Wall3D, id);
        public ITexture LoadTexture(FontId id)                     => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.Font, id);
        public ITexture LoadTexture(FullBodyPictureId id)          => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.FullBodyPicture, id);
        public ITexture LoadTexture(IconGraphicsId id)             => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.IconGraphics, id);
        public ITexture LoadTexture(ItemSpriteId id)               => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.ItemGraphics, 0);
        public ITexture LoadTexture(LargeNpcId id)                 => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.BigNpcGraphics, id);
        public ITexture LoadTexture(LargePartyGraphicsId id)       => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.BigPartyGraphics, id);
        public ITexture LoadTexture(MetaFontId id)                 => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.MetaFont, id);
        public ITexture LoadTexture(MonsterGraphicsId id)          => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.MonsterGraphics, id);
        public ITexture LoadTexture(PictureId id)                  => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.Picture, id);
        public ITexture LoadTexture(SmallNpcId id)                 => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.SmallNpcGraphics, id);
        public ITexture LoadTexture(SmallPartyGraphicsId id)       => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.SmallPartyGraphics, id);
        public ITexture LoadTexture(SmallPortraitId id)            => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.SmallPortrait, id);
        public ITexture LoadTexture(TacticId id)                   => (ITexture)_assetLocatorRegistry.LoadAssetCached(AssetType.TacticalIcon, id);
        public TilesetData LoadTileData(TilesetId id)              => (TilesetData)_assetLocatorRegistry.LoadAssetCached(AssetType.Tileset, id);
        public LabyrinthData LoadLabyrinthData(LabyrinthDataId id) => (LabyrinthData)_assetLocatorRegistry.LoadAssetCached(AssetType.LabData, id);
        public ITexture LoadFont(FontColor color, bool isBold)     => LoadTexture(new MetaFontId(isBold, color));

        public IAssetConfig LoadAssetConfig() => (IAssetConfig) _assetLocatorRegistry.LoadAssetCached(AssetType.AssetConfig, 0);
        public IGeneralConfig LoadGeneralConfig() => (IGeneralConfig) _assetLocatorRegistry.LoadAssetCached(AssetType.GeneralConfig, 0);
        public CoreSpriteConfig.BinaryResource LoadCoreSpriteInfo(CoreSpriteId id) =>
            (CoreSpriteConfig.BinaryResource)_assetLocatorRegistry.LoadAssetCached(AssetType.CoreGraphicsMetadata, id);

        public string LoadString(StringId id, GameLanguage language)
        {
            var asset = _assetLocatorRegistry.LoadAssetCached(id.Type, id.Id, language);

            if (asset is string s)
                return s;

            if (!(asset is IDictionary<int, string> stringTable))
                return $"!MISSING STRING-TABLE {id.Type}:{id.Id}:{id.SubId}:{language}!";

            return stringTable.TryGetValue(id.SubId, out var value)
                ? value
                : $"!MISSING STRING {id.Type}:{id.Id}:{id.SubId}:{language}!";
        }

        public string LoadString(SystemTextId id, GameLanguage language) => LoadString(new StringId(AssetType.SystemText, 0, (int)id), language);
        public string LoadString(WordId id, GameLanguage language) => LoadString(new StringId(AssetType.Dictionary, (int)id / 500, (int)id), language);

        public ISample LoadSample(SampleId id) => (AlbionSample)_assetLocatorRegistry.LoadAssetCached(AssetType.Sample, id);

        public ISample LoadWaveLib(SongId songId, int instrument)
            => ((WaveLib) _assetLocatorRegistry.LoadAssetCached(AssetType.WaveLibrary, songId))?.GetSample(instrument);

        public byte[] LoadSoundBanks() => (byte[]) _assetLocatorRegistry.LoadAssetCached(AssetType.SoundBank, 0);
        public AlbionVideo LoadVideo(VideoId id, GameLanguage language) => (AlbionVideo)_assetLocatorRegistry.LoadAssetCached(AssetType.Flic, (int)id, language);
        public CharacterSheet LoadCharacter(PartyCharacterId id) => (CharacterSheet)_assetLocatorRegistry.LoadAssetCached(AssetType.PartyMember, id);
        public CharacterSheet LoadCharacter(NpcCharacterId id) => (CharacterSheet)_assetLocatorRegistry.LoadAssetCached(AssetType.Npc, id);
        public CharacterSheet LoadCharacter(MonsterCharacterId id) => (CharacterSheet)_assetLocatorRegistry.LoadAssetCached(AssetType.Monster, id);
        public Inventory LoadChest(ChestId id) => (Inventory)_assetLocatorRegistry.LoadAssetCached(AssetType.ChestData, id);
        public Inventory LoadMerchant(MerchantId id)=> (Inventory)_assetLocatorRegistry.LoadAssetCached(AssetType.MerchantData, id);
        public WordId? ParseWord(string word)
        {
            var words = // Inefficient code, if it ends up being a problem then we can build a reverse dictionary and cache it.
                new[]
                {   // Load the english files as all languages use english {WORDxxx} tags
                    (IDictionary<int, string>) _assetLocatorRegistry.LoadAssetCached(AssetType.Dictionary, 0),
                    (IDictionary<int, string>) _assetLocatorRegistry.LoadAssetCached(AssetType.Dictionary, 1),
                    (IDictionary<int, string>) _assetLocatorRegistry.LoadAssetCached(AssetType.Dictionary, 2)
                };

            return words
                .SelectMany(x => x)
                .Where(x => x.Value == word)
                .Select(x => (WordId?)x.Key)
                .FirstOrDefault();
        }

        public IList<Block> LoadBlockList(BlockListId blockListId) => (IList<Block>)_assetLocatorRegistry.LoadAssetCached(AssetType.BlockList, blockListId);
        public EventSet LoadEventSet(EventSetId eventSetId) => (EventSet)_assetLocatorRegistry.LoadAssetCached(AssetType.EventSet, eventSetId);
        public byte[] LoadSong(SongId songId) => (byte[]) _assetLocatorRegistry.LoadAssetCached(AssetType.Song, songId);
        public IList<IEvent> LoadScript(ScriptId scriptId) => (IList<IEvent>) _assetLocatorRegistry.LoadAsset(AssetType.Script, scriptId);
    }
}
