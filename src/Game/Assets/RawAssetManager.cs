using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    /// <summary>
    /// An asset manager that always returns a new copy of requested assets
    /// without using any caching layer. Primarily used in the editor mode.
    /// </summary>
    public class RawAssetManager : Component, IRawAssetManager
    {
        IAssetLocatorRegistry _assetLocatorRegistry;

        protected override void Subscribed()
        {
            _assetLocatorRegistry = Resolve<IAssetLocatorRegistry>();
            Exchange.Register<IRawAssetManager>(this);
        }

        protected override void Unsubscribed() => Exchange.Unregister(this);

        public void Save(AssetKey key, object asset)
        {
        }

        public IEnumerable<AssetKey> EnumerateAssets(AssetType type)
        {
            var enumType = AssetNameResolver.GetEnumType(type);
            if (enumType == null)
                yield break;

            var values = Enum.GetValues(enumType);
            foreach(var value in values)
                yield return new AssetKey(type, Convert.ToUInt16(value, CultureInfo.InvariantCulture));
        }

        public AssetInfo GetAssetInfo(AssetKey key) => _assetLocatorRegistry.GetAssetInfo(key);
        public IMapData LoadMap(MapDataId id) => (IMapData)_assetLocatorRegistry.LoadAsset(id.ToAssetId()); // No caching for map data
        public ItemData LoadItem(ItemId id)
        {
            var data = (IList<ItemData>)_assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.ItemList));
            if ((int)id >= data.Count)
                return null;

            return data[(int)id];
        }

        public AlbionPalette LoadPalette(PaletteId id)
        {
            var palette = (AlbionPalette)_assetLocatorRegistry.LoadAsset(id.ToAssetId());
            if (palette == null)
                return null;

            var commonPalette = (byte[])_assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.PaletteNull));
            palette.SetCommonPalette(commonPalette);

            return palette;
        }

        // public ITexture LoadTexture(AssetType type, ushort id) => (ITexture)_assetLocatorRegistry.LoadAsset(type, id);

        public ITexture LoadTexture<T>(T id) => (ITexture)(id switch
        {
            AssetId x                    => _assetLocatorRegistry.LoadAsset(x),
            AutoMapId x                  => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            AutoGraphicsId x             => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            CombatBackgroundId x         => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            CombatGraphicsId x           => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            CoreSpriteId x               => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            DungeonBackgroundId x        => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            DungeonFloorId x             => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            DungeonObjectId x            => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            DungeonOverlayId x           => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            DungeonWallId x              => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            FontId x                     => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            FullBodyPictureId x          => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            IconGraphicsId x             => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            ItemSpriteId _ => _assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.ItemGraphics)),
            LargeNpcId x                 => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            LargePartyGraphicsId x       => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            MetaFontId x => _assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.MetaFont, (ushort)x)),
            MonsterGraphicsId x          => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            PictureId x                  => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            SmallNpcId x                 => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            SmallPartyGraphicsId x       => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            SmallPortraitId x            => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            TacticId x                   => _assetLocatorRegistry.LoadAsset(x.ToAssetId()),
            SlabId _ => _assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.Slab)),
            _ => throw new ArgumentOutOfRangeException(nameof(id), $"Expected texture id, but given a {typeof(T)}")
        });

        public ITexture LoadFont(FontColor color, bool isBold) 
            => (ITexture)_assetLocatorRegistry.LoadAsset(new AssetKey(
                AssetType.MetaFont, (ushort)new MetaFontId(isBold, color)));

        public TilesetData LoadTileData(TilesetId id) => (TilesetData)_assetLocatorRegistry.LoadAsset(id.ToAssetId());
        public LabyrinthData LoadLabyrinthData(LabyrinthDataId id) => (LabyrinthData)_assetLocatorRegistry.LoadAsset(id.ToAssetId());
        public IAssetConfig LoadAssetConfig() => (IAssetConfig) _assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.AssetConfig));
        public IGeneralConfig LoadGeneralConfig() => (IGeneralConfig) _assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.GeneralConfig));
        public CoreSpriteInfo LoadCoreSpriteInfo(CoreSpriteId id) => (CoreSpriteInfo)_assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.CoreGraphicsMetadata, (ushort)id));

        public string LoadString(StringId id, GameLanguage language)
        {
            var asset = _assetLocatorRegistry.LoadAsset(new AssetKey(id.Type, id.Id, language));
            return (asset switch
            {
                string s => s,
                IDictionary<int, string> d => d.GetValueOrDefault(id.SubId),
                IDictionary<(int, GameLanguage), string> d => d.GetValueOrDefault((id.Id, language)),
                _ => $"!MISSING STRING-TABLE {id.Type}:{id.Id}:{id.SubId}:{language}!"
            }) ?? $"!MISSING STRING {id.Type}:{id.Id}:{id.SubId}:{language}!";
        }

        public ISample LoadSample(SampleId id) => (AlbionSample)_assetLocatorRegistry.LoadAsset(id.ToAssetId());
        public ISample LoadWaveLib(SongId songId, int instrument)
            => ((WaveLib)_assetLocatorRegistry.LoadAsset(
                new AssetKey(AssetType.WaveLibrary, (ushort)songId)
            ))?.GetSample(instrument);

        public byte[] LoadSoundBanks() => (byte[]) _assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.SoundBank));
        public FlicFile LoadVideo(VideoId id, GameLanguage language) => (FlicFile)_assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.Flic, (ushort)id, language));
        public CharacterSheet LoadPartyMember(PartyCharacterId id) => (CharacterSheet)_assetLocatorRegistry.LoadAsset(id.ToAssetId());
        public CharacterSheet LoadNpc(NpcCharacterId id) => (CharacterSheet)_assetLocatorRegistry.LoadAsset(id.ToAssetId());
        public CharacterSheet LoadMonster(MonsterCharacterId id) => (CharacterSheet)_assetLocatorRegistry.LoadAsset(id.ToAssetId());
        public Inventory LoadChest(ChestId id) => (Inventory)_assetLocatorRegistry.LoadAsset(id.ToAssetId());
        public Inventory LoadMerchant(MerchantId id) => (Inventory)_assetLocatorRegistry.LoadAsset(id.ToAssetId());
        public WordId? ParseWord(string word)
        {
            var words = // Inefficient code, if it ends up being a problem then we can build a reverse dictionary and cache it.
                new[]
                {   // Load the english files as all languages use english {WORDxxx} tags
                    (IDictionary<int, string>) _assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.Dictionary)),
                    (IDictionary<int, string>) _assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.Dictionary, 1)),
                    (IDictionary<int, string>) _assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.Dictionary, 2))
                };

            return words
                .SelectMany(x => x)
                .Where(x => x.Value == word)
                .Select(x => (WordId?)x.Key)
                .FirstOrDefault();
        }

        public IList<Block> LoadBlockList(BlockListId blockListId) => (IList<Block>)_assetLocatorRegistry.LoadAsset(blockListId.ToAssetId());
        public EventSet LoadEventSet(EventSetId eventSetId) => (EventSet)_assetLocatorRegistry.LoadAsset(eventSetId.ToAssetId());
        public byte[] LoadSong(SongId songId) => (byte[]) _assetLocatorRegistry.LoadAsset(songId.ToAssetId());
        public IList<IEvent> LoadScript(ScriptId scriptId) => (IList<IEvent>) _assetLocatorRegistry.LoadAsset(scriptId.ToAssetId());

        public SpellData LoadSpell(SpellId spellId)
        {
            var spells = (IList<SpellData>)_assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.SpellData));
            return spells[(int)spellId];
        }
        public SavedGame LoadSavedGame(ushort id) 
            => (SavedGame)_assetLocatorRegistry.LoadAsset(new AssetKey(AssetType.SavedGame, id));

        public MonsterGroup LoadMonsterGroup(MonsterGroupId groupId)
            => (MonsterGroup)_assetLocatorRegistry.LoadAsset(groupId.ToAssetId());
    }
}