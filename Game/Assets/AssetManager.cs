using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
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

        protected override void Subscribed()
        {
            Exchange.Register<IAssetManager>(this);
            Exchange.Register<ITextureLoader>(this);
        }

        protected override void Unsubscribed()
        {
            Exchange.Unregister<IAssetManager>(this);
            Exchange.Unregister<ITextureLoader>(this);
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
        public IMapData LoadMap(MapDataId id) => (IMapData)_assetLocatorRegistry.LoadAsset(id.ToAssetId()); // No caching for map data
        public ItemData LoadItem(ItemId id)
        {
            var data = (IList<ItemData>)_assetLocatorRegistry.LoadAssetCached(AssetType.ItemList, 0);
            if ((int)id >= data.Count)
                return null;

            return data[(int)id];
        }

        public AlbionPalette LoadPalette(PaletteId id)
        {
            var palette = (AlbionPalette)_assetLocatorRegistry.LoadAssetCached(id.ToAssetId());
            if (palette == null)
                return null;

            var commonPalette = (byte[])_assetLocatorRegistry.LoadAssetCached(AssetType.PaletteNull, 0);
            palette.SetCommonPalette(commonPalette);

            return palette;
        }

        public ITexture LoadTexture(AssetType type, ushort id) =>
            (ITexture)_assetLocatorRegistry.LoadAssetCached(type, id);

        /*(ITexture)(type switch
            {
                AssetType.AutomapGraphics    => _assetLocatorRegistry.LoadAssetCached(AssetType.AutomapGraphics, id),
                AssetType.CombatBackground   => _assetLocatorRegistry.LoadAssetCached(AssetType.CombatBackground, id),
                AssetType.CombatGraphics     => _assetLocatorRegistry.LoadAssetCached(AssetType.CombatGraphics, id),
                AssetType.CoreGraphics       => _assetLocatorRegistry.LoadAssetCached(AssetType.CoreGraphics, id),
                AssetType.BackgroundGraphics => _assetLocatorRegistry.LoadAssetCached(AssetType.BackgroundGraphics, id),
                AssetType.Floor3D            => _assetLocatorRegistry.LoadAssetCached(AssetType.Floor3D, id),
                AssetType.Object3D           => _assetLocatorRegistry.LoadAssetCached(AssetType.Object3D, id),
                AssetType.Overlay3D          => _assetLocatorRegistry.LoadAssetCached(AssetType.Overlay3D, id),
                AssetType.Wall3D             => _assetLocatorRegistry.LoadAssetCached(AssetType.Wall3D, id),
                AssetType.Font               => _assetLocatorRegistry.LoadAssetCached(AssetType.Font, id),
                AssetType.FullBodyPicture    => _assetLocatorRegistry.LoadAssetCached(AssetType.FullBodyPicture, id),
                AssetType.IconGraphics       => _assetLocatorRegistry.LoadAssetCached(AssetType.IconGraphics, id),
                AssetType.ItemGraphics       => _assetLocatorRegistry.LoadAssetCached(AssetType.ItemGraphics, 0),
                AssetType.BigNpcGraphics     => _assetLocatorRegistry.LoadAssetCached(AssetType.BigNpcGraphics, id),
                AssetType.BigPartyGraphics   => _assetLocatorRegistry.LoadAssetCached(AssetType.BigPartyGraphics, id),
                AssetType.MetaFont           => _assetLocatorRegistry.LoadAssetCached(AssetType.MetaFont, id),
                AssetType.MonsterGraphics    => _assetLocatorRegistry.LoadAssetCached(AssetType.MonsterGraphics, id),
                AssetType.Picture            => _assetLocatorRegistry.LoadAssetCached(AssetType.Picture, id),
                AssetType.SmallNpcGraphics   => _assetLocatorRegistry.LoadAssetCached(AssetType.SmallNpcGraphics, id),
                AssetType.SmallPartyGraphics => _assetLocatorRegistry.LoadAssetCached(AssetType.SmallPartyGraphics, id),
                AssetType.SmallPortrait      => _assetLocatorRegistry.LoadAssetCached(AssetType.SmallPortrait, id),
                AssetType.TacticalIcon       => _assetLocatorRegistry.LoadAssetCached(AssetType.TacticalIcon, id),
                AssetType.Slab               => _assetLocatorRegistry.LoadAssetCached(AssetType.Slab, 0),
                _ => _assetLocatorRegistry.LoadAssetCached(type, id)
            });*/

        public ITexture LoadTexture<T>(T id) => (ITexture)(id switch
        {
            AssetId x                    => _assetLocatorRegistry.LoadAssetCached(x),
            AutoMapId x                  => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            CombatBackgroundId x         => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            CombatGraphicsId x           => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            CoreSpriteId x               => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            DungeonBackgroundId x        => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            DungeonFloorId x             => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            DungeonObjectId x            => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            DungeonOverlayId x           => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            DungeonWallId x              => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            FontId x                     => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            FullBodyPictureId x          => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            IconGraphicsId x             => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            ItemSpriteId _ => _assetLocatorRegistry.LoadAssetCached(AssetType.ItemGraphics, 0),
            LargeNpcId x                 => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            LargePartyGraphicsId x       => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            MetaFontId x => _assetLocatorRegistry.LoadAssetCached(AssetType.MetaFont, (ushort)x),
            MonsterGraphicsId x          => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            PictureId x                  => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            SmallNpcId x                 => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            SmallPartyGraphicsId x       => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            SmallPortraitId x            => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            TacticId x                   => _assetLocatorRegistry.LoadAssetCached(x.ToAssetId()),
            SlabId _ => _assetLocatorRegistry.LoadAssetCached(AssetType.Slab, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(id), $"Expected texture id, but given a {typeof(T)}")
        });

        public ITexture LoadFont(FontColor color, bool isBold) 
            => (ITexture)_assetLocatorRegistry.LoadAssetCached(
                AssetType.MetaFont, (ushort)new MetaFontId(isBold, color));

        public TilesetData LoadTileData(TilesetId id) => (TilesetData)_assetLocatorRegistry.LoadAssetCached(id.ToAssetId());
        public LabyrinthData LoadLabyrinthData(LabyrinthDataId id) => (LabyrinthData)_assetLocatorRegistry.LoadAssetCached(id.ToAssetId());
        public IAssetConfig LoadAssetConfig() => (IAssetConfig) _assetLocatorRegistry.LoadAssetCached(AssetType.AssetConfig, 0);
        public IGeneralConfig LoadGeneralConfig() => (IGeneralConfig) _assetLocatorRegistry.LoadAssetCached(AssetType.GeneralConfig, 0);
        public CoreSpriteConfig.BinaryResource LoadCoreSpriteInfo(CoreSpriteId id) =>
            (CoreSpriteConfig.BinaryResource)_assetLocatorRegistry.LoadAssetCached(AssetType.CoreGraphicsMetadata, (ushort)id);

        public string LoadString(StringId id, GameLanguage language)
        {
            var asset = _assetLocatorRegistry.LoadAssetCached(new AssetKey(id.Type, id.Id, language));
            return (asset switch
            {
                string s => s,
                IDictionary<int, string> d => d.GetValueOrDefault(id.SubId),
                IDictionary<(int, GameLanguage), string> d => d.GetValueOrDefault((id.Id, language)),
                _ => $"!MISSING STRING-TABLE {id.Type}:{id.Id}:{id.SubId}:{language}!"
            }) ?? $"!MISSING STRING {id.Type}:{id.Id}:{id.SubId}:{language}!";
        }

        public string LoadString(SystemTextId id, GameLanguage language) => LoadString(id.ToId(), language);
        public ISample LoadSample(SampleId id) => (AlbionSample)_assetLocatorRegistry.LoadAssetCached(id.ToAssetId());
        public ISample LoadWaveLib(SongId songId, int instrument)
            => ((WaveLib)_assetLocatorRegistry.LoadAssetCached(AssetType.WaveLibrary, (ushort)songId))?.GetSample(instrument);

        public byte[] LoadSoundBanks() => (byte[]) _assetLocatorRegistry.LoadAssetCached(AssetType.SoundBank, 0);
        public AlbionVideo LoadVideo(VideoId id, GameLanguage language) => (AlbionVideo)_assetLocatorRegistry.LoadAssetCached(new AssetKey(AssetType.Flic, (ushort)id, language));
        public CharacterSheet LoadCharacter(PartyCharacterId id) => (CharacterSheet)_assetLocatorRegistry.LoadAssetCached(id.ToAssetId());
        public CharacterSheet LoadCharacter(NpcCharacterId id) => (CharacterSheet)_assetLocatorRegistry.LoadAssetCached(id.ToAssetId());
        public CharacterSheet LoadCharacter(MonsterCharacterId id) => (CharacterSheet)_assetLocatorRegistry.LoadAssetCached(id.ToAssetId());
        public Inventory LoadChest(ChestId id) => (Inventory)_assetLocatorRegistry.LoadAssetCached(id.ToAssetId());
        public Inventory LoadMerchant(MerchantId id) => (Inventory)_assetLocatorRegistry.LoadAssetCached(id.ToAssetId());
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

        public IList<Block> LoadBlockList(BlockListId blockListId) => (IList<Block>)_assetLocatorRegistry.LoadAssetCached(blockListId.ToAssetId());
        public EventSet LoadEventSet(EventSetId eventSetId) => (EventSet)_assetLocatorRegistry.LoadAssetCached(eventSetId.ToAssetId());
        public byte[] LoadSong(SongId songId) => (byte[]) _assetLocatorRegistry.LoadAssetCached(songId.ToAssetId());
        public IList<IEvent> LoadScript(ScriptId scriptId) => (IList<IEvent>) _assetLocatorRegistry.LoadAsset(scriptId.ToAssetId());

        public SpellData LoadSpell(SpellId spellId)
        {
            var spells = (IList<SpellData>)_assetLocatorRegistry.LoadAssetCached(AssetType.SpellData, 0);
            return spells[(int)spellId];
        }
    }
}
