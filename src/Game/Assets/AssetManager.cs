using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class AssetManager : Component, IAssetManager
    {
        IModApplier _modApplier;

        protected override void Subscribed()
        {
            _modApplier = Resolve<IModApplier>() ?? throw new InvalidOperationException("AssetManager is missing requirement of type IModApplier");
            Exchange.Register<IAssetManager>(this);
            Exchange.Register<ITextureLoader>(this);
        }

        protected override void Unsubscribed() => Exchange.Unregister(this);
        public AssetInfo GetAssetInfo(AssetId id) => _modApplier.GetAssetInfo(id);
        public IMapData LoadMap(MapId id) => (IMapData)_modApplier.LoadAsset(id); // No caching for map data
        public ItemData LoadItem(ItemId id)
        {
            return (ItemData)_modApplier.LoadAssetCached(id);
            /*var data = (IList<ItemData>)_modApplier.LoadAssetCached(ItemListId.Only.Id());
            if ((int)id >= data.Count)
                return null;

            return data[(int)id]; */
        }

        public AlbionPalette LoadPalette(PaletteId id)
        {
            var palette = (AlbionPalette)_modApplier.LoadAssetCached(id);
            if (palette == null)
                return null;

            var commonPalette = (AlbionPalette)_modApplier.LoadAssetCached(AssetId.From(Base.Palette.CommonPalette));
            palette.SetCommonPalette(commonPalette);

            return palette;
        }

        public ITexture LoadTexture(SpriteId id) => (ITexture)_modApplier.LoadAssetCached(id);
        public ITexture LoadTexture(ITextureId id) => (ITexture)_modApplier.LoadAssetCached(SpriteId.FromUInt32(id?.ToUInt32() ?? 0));
        public ITexture LoadFont(FontColor color, bool isBold) 
            => (ITexture)_modApplier.LoadAssetCached(new AssetId(
                AssetType.MetaFont, (ushort)new MetaFontId(isBold, color)));

        TilesetData IAssetManager.LoadTileData(TilesetId id)
        {
            return LoadTileData(id);
        }

        LabyrinthData IAssetManager.LoadLabyrinthData(LabyrinthId id)
        {
            return LoadLabyrinthData(id);
        }

        public TilesetData LoadTileData(TilesetId id) => (TilesetData)_modApplier.LoadAssetCached(id);
        public LabyrinthData LoadLabyrinthData(LabyrinthId id) => (LabyrinthData)_modApplier.LoadAssetCached(id);

        public string LoadString(TextId id) => LoadString((StringId)id);
        public string LoadString(StringId id)
        {
            var asset = _modApplier.LoadAssetCached(id.Id);
            return (asset switch
            {
                string s => s,
                IDictionary<int, string> d => d.GetValueOrDefault(id.SubId),
                IDictionary<AssetId, string> d => d.GetValueOrDefault(id.Id),
                _ => $"!MISSING STRING-TABLE {id.Id}:{id.SubId}!"
            }) ?? $"!MISSING STRING {id.Id}:{id.SubId}!";
        }

        public ISample LoadSample(SampleId id) => (AlbionSample)_modApplier.LoadAssetCached(id);
        public WaveLib LoadWaveLib(WaveLibraryId waveLibraryId) => (WaveLib)_modApplier.LoadAssetCached(waveLibraryId);
        public FlicFile LoadVideo(VideoId id) => (FlicFile)_modApplier.LoadAssetCached(id);
        public CharacterSheet LoadSheet(CharacterId id) => (CharacterSheet)_modApplier.LoadAssetCached(id);
        public Inventory LoadInventory(AssetId id) => (Inventory)_modApplier.LoadAssetCached(id);
        public WordId? ParseWord(string word)
        {
            throw new NotImplementedException();
            /*
            var words = // Inefficient code, if it ends up being a problem then we can build a reverse dictionary and cache it.
                new[]
                {   // Load the English files as all languages use English {WORDxxx} tags
                    (IDictionary<int, string>) _modApplier.LoadAssetCached(new AssetId(AssetType.Dictionary)),
                    (IDictionary<int, string>) _modApplier.LoadAssetCached(new AssetId(AssetType.Dictionary, 1)),
                    (IDictionary<int, string>) _modApplier.LoadAssetCached(new AssetId(AssetType.Dictionary, 2))
                };

            return words
                .SelectMany(x => x)
                .Where(x => x.Value == word)
                .Select(x => (WordId?)x.Key)
                .FirstOrDefault();
            */
        }

        public IList<Block> LoadBlockList(BlockListId id) => (IList<Block>)_modApplier.LoadAssetCached(id);
        public EventSet LoadEventSet(EventSetId id) => (EventSet)_modApplier.LoadAssetCached(id);
        public byte[] LoadSong(SongId id) => (byte[]) _modApplier.LoadAssetCached(id);
        public IList<IEvent> LoadScript(ScriptId id) => (IList<IEvent>) _modApplier.LoadAsset(id);

        public SpellData LoadSpell(SpellId id) => (SpellData)_modApplier.LoadAssetCached(id);
        public SavedGame LoadSavedGame(string path) => _modApplier.LoadSavedGame(path);
        public MonsterGroup LoadMonsterGroup(MonsterGroupId id) => (MonsterGroup)_modApplier.LoadAssetCached(id);
        public Automap LoadAutomap(AutomapId id) => (Automap)_modApplier.LoadAssetCached(id);

        public GameConfig LoadGameConfig() => (GameConfig)_modApplier.LoadAssetCached(AssetId.From(Base.Special.GameConfig));
        public CoreConfig LoadCoreConfig()=> (CoreConfig)_modApplier.LoadAssetCached(AssetId.From(Base.Special.CoreConfig));
        public byte[] LoadSoundBanks() => (byte[]) _modApplier.LoadAssetCached(AssetId.From(Base.Special.SoundBank));
    }
}
