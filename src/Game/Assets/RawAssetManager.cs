using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Assets;

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

    public IEnumerable<AssetId> EnumerateAssets(AssetType type) => AssetMapping.Global.EnumerateAssetsOfType(type);
    public AssetInfo GetAssetInfo(AssetId id, string language = null) => _modApplier.GetAssetInfo(id, language);
    public IMapData LoadMap(MapId id) => (IMapData)_modApplier.LoadAsset(id); // No caching for map data
    public ItemData LoadItem(ItemId id) => (ItemData)_modApplier.LoadAsset(id);

    public AlbionPalette LoadPalette(PaletteId id)
    {
        var palette = (AlbionPalette)_modApplier.LoadAsset(id);
        if (palette == null)
            return null;

        var commonPalette = (AlbionPalette)_modApplier.LoadAssetCached(AssetId.From(Base.Palette.Common));
        palette.SetCommonPalette(commonPalette);

        return palette;
    }

    public ITexture LoadTexture(SpriteId id) => (ITexture)_modApplier.LoadAsset(id);
    public ITexture LoadTexture(IAssetId id) => (ITexture)_modApplier.LoadAsset(SpriteId.FromUInt32(id?.ToUInt32() ?? 0));
    public ITexture LoadFont(FontColor color, bool isBold) 
        => (ITexture)_modApplier.LoadAsset(new AssetId(
            AssetType.MetaFont, (ushort)new MetaFontId(isBold, color)));

    public ITileGraphics LoadTileGraphics(TilesetGraphicsId id) => (ITileGraphics)_modApplier.LoadAsset(id);
    public TilesetData LoadTileData(TilesetId id) => (TilesetData)_modApplier.LoadAsset(id);
    public LabyrinthData LoadLabyrinthData(LabyrinthId id) => (LabyrinthData)_modApplier.LoadAsset(id);

    string LoadStringCore(StringId id, string language)
    {
        language ??= Resolve<IGameplaySettings>().Language;
        var asset = _modApplier.LoadAsset(id.Id, language);
        return asset switch
        {
            IStringCollection collection => collection.GetString(id, language),
            string s => s,
            _ => null
        };
    }
    public bool IsStringDefined(TextId id, string language) => LoadStringCore(id, language) != null;
    public bool IsStringDefined(StringId id, string language) => LoadStringCore(id, language) != null;
    public string LoadString(TextId id) => LoadString((StringId)id, null);
    public string LoadString(StringId id) => LoadString(id, null);
    public string LoadString(TextId id, string language) => LoadString((StringId)id, language);
    public string LoadString(StringId id, string language) => LoadStringCore(id, language) // Raw manager - not cached
                                                              ?? $"!MISSING STRING {id.Id}:{id.SubId}!";

    public ISample LoadSample(SampleId id) => (AlbionSample)_modApplier.LoadAsset(id);
    public WaveLib LoadWaveLib(WaveLibraryId waveLibraryId) => (WaveLib)_modApplier.LoadAsset(waveLibraryId);
    public FlicFile LoadVideo(VideoId id) => (FlicFile)_modApplier.LoadAsset(id);
    public CharacterSheet LoadSheet(CharacterId id) => (CharacterSheet)_modApplier.LoadAsset(id);
    public Inventory LoadInventory(AssetId id) => (Inventory)_modApplier.LoadAsset(id);
    public IList<Block> LoadBlockList(BlockListId blockListId) => (IList<Block>)_modApplier.LoadAsset(blockListId);
    public EventSet LoadEventSet(EventSetId eventSetId) => (EventSet)_modApplier.LoadAsset(eventSetId);
    public byte[] LoadSong(SongId songId) => (byte[]) _modApplier.LoadAsset(songId);
    public IList<IEvent> LoadScript(ScriptId scriptId) => (IList<IEvent>) _modApplier.LoadAsset(scriptId);
    public SpellData LoadSpell(SpellId id) => (SpellData)_modApplier.LoadAsset(id);
    public SavedGame LoadSavedGame(string path) => _modApplier.LoadSavedGame(path);
    public MonsterGroup LoadMonsterGroup(MonsterGroupId id) => (MonsterGroup)_modApplier.LoadAsset(id);
    public Automap LoadAutomap(AutomapId id) => (Automap) _modApplier.LoadAsset(id);
    public byte[] LoadSoundBanks() => (byte[]) _modApplier.LoadAsset(AssetId.From(Base.Special.SoundBank));
}