using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.Assets
{
    /// <summary>
    /// An asset manager that always returns a new copy of requested assets
    /// without using any caching layer. Primarily used in the editor mode.
    /// </summary>
    public class RawAssetManager : Component, IRawAssetManager
    {
        IModApplier _modApplier;
        protected override void Subscribed()
        {
            _modApplier = Resolve<IModApplier>();
            Exchange.Register<IRawAssetManager>(this);
        }

        protected override void Unsubscribed() => Exchange.Unregister(this);

        public void Save(AssetId id, object asset)
        {
        }

        public IEnumerable<AssetId> EnumerateAssets(AssetType type) => AssetMapping.Global.EnumeratAssetsOfType(type);
        public AssetInfo GetAssetInfo(AssetId id) => _modApplier.GetAssetInfo(id);
        public IMapData LoadMap(MapId id) => (IMapData)_modApplier.LoadAsset(id); // No caching for map data
        public ItemData LoadItem(ItemId id) => (ItemData)_modApplier.LoadAsset(id);

        public AlbionPalette LoadPalette(PaletteId id)
        {
            var palette = (AlbionPalette)_modApplier.LoadAsset(id);
            if (palette == null)
                return null;

            var commonPalette = (byte[])_modApplier.LoadAsset(AssetId.From(Base.Special.CommonPalette));
            palette.SetCommonPalette(commonPalette);

            return palette;
        }

        public ITexture LoadTexture(SpriteId id) => (ITexture)_modApplier.LoadAsset(id);
        public ITexture LoadTexture(ITextureId id) => (ITexture)_modApplier.LoadAsset(new SpriteId(id?.ToUInt32() ?? 0));
        public ITexture LoadFont(FontColor color, bool isBold) 
            => (ITexture)_modApplier.LoadAsset(new AssetId(
                AssetType.MetaFont, (ushort)new MetaFontId(isBold, color)));

        public TilesetData LoadTileData(TilesetId id) => (TilesetData)_modApplier.LoadAsset(id);
        public LabyrinthData LoadLabyrinthData(LabyrinthId id) => (LabyrinthData)_modApplier.LoadAsset(id);
        public IGeneralConfig LoadGeneralConfig() => (IGeneralConfig) _modApplier.LoadAsset(AssetId.From(Base.Special.GeneralConfig));
        public string LoadString(TextId id) => LoadString((StringId)id);
        public string LoadString(StringId id)
        {
            var asset = _modApplier.LoadAsset(id.Id);
            return (asset switch
            {
                string s => s,
                IDictionary<int, string> d => d.GetValueOrDefault(id.SubId),
                IDictionary<AssetId, string> d => d.GetValueOrDefault(id.Id),
                _ => $"!MISSING STRING-TABLE {id.Id}:{id.SubId}!"
            }) ?? $"!MISSING STRING {id.Id}:{id.SubId}!";
        }

        public ISample LoadSample(SampleId id) => (AlbionSample)_modApplier.LoadAsset(id);
        public ISample LoadWaveLib(WaveLibraryId waveLibraryId, int instrument) => ((WaveLib)_modApplier.LoadAsset(waveLibraryId))?.GetSample(instrument);
        public byte[] LoadSoundBanks() => (byte[]) _modApplier.LoadAsset(AssetId.From(Base.Special.SoundBank));
        public FlicFile LoadVideo(VideoId id) => (FlicFile)_modApplier.LoadAsset(id);
        public CharacterSheet LoadSheet(CharacterId id) => (CharacterSheet)_modApplier.LoadAsset(id);
        public Inventory LoadInventory(AssetId id) => (Inventory)_modApplier.LoadAsset(id);
        public WordId? ParseWord(string word)
        {
            var words = // Inefficient code, if it ends up being a problem then we can build a reverse dictionary and cache it.
                new[]
                {   // Load the english files as all languages use english {WORDxxx} tags
                    (IDictionary<int, string>) _modApplier.LoadAsset(new AssetId(AssetType.Dictionary)),
                    (IDictionary<int, string>) _modApplier.LoadAsset(new AssetId(AssetType.Dictionary, 1)),
                    (IDictionary<int, string>) _modApplier.LoadAsset(new AssetId(AssetType.Dictionary, 2))
                };

            return words
                .SelectMany(x => x)
                .Where(x => x.Value == word)
                .Select(x => (WordId?)x.Key)
                .FirstOrDefault();
        }

        public IList<Block> LoadBlockList(BlockListId blockListId) => (IList<Block>)_modApplier.LoadAsset(blockListId);
        public EventSet LoadEventSet(EventSetId eventSetId) => (EventSet)_modApplier.LoadAsset(eventSetId);
        public byte[] LoadSong(SongId songId) => (byte[]) _modApplier.LoadAsset(songId);
        public IList<IEvent> LoadScript(ScriptId scriptId) => (IList<IEvent>) _modApplier.LoadAsset(scriptId);
        public SpellData LoadSpell(SpellId id) => (SpellData)_modApplier.LoadAsset(id);
        public SavedGame LoadSavedGame(string path) => _modApplier.LoadSavedGame(path);
        public MonsterGroup LoadMonsterGroup(MonsterGroupId id) => (MonsterGroup)_modApplier.LoadAsset(id);
    }
}
