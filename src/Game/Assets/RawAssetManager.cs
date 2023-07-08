using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;
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

    public void Save(AssetId key, object asset) { }

    public IEnumerable<AssetId> EnumerateAssets(AssetType type) => AssetMapping.Global.EnumerateAssetsOfType(type);
    public AssetNode GetAssetInfo(AssetId id, string language = null) => _modApplier.GetAssetInfo(id, language);
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
    public object LoadMapObject(MapObjectId id) => _modApplier.LoadAsset(id);
    public Ink LoadInk(InkId id) => (Ink)_modApplier.LoadAsset(id);
    public FontDefinition LoadFontDefinition(FontId id) => (FontDefinition)_modApplier.LoadAsset(id);
    public MetaFont LoadFont(FontId fontId, InkId inkId) => (MetaFont)_modApplier.LoadAsset(new MetaFontId(fontId, inkId));

    public ITileGraphics LoadTileGraphics(TilesetGfxId id) => (ITileGraphics)_modApplier.LoadAsset(id);
    public TilesetData LoadTileData(TilesetId id) => (TilesetData)_modApplier.LoadAsset(id);
    public LabyrinthData LoadLabyrinthData(LabyrinthId id) => (LabyrinthData)_modApplier.LoadAsset(id);

    public IStringSet LoadStringSet(StringSetId id) => LoadStringSet(id, null);
    public IStringSet LoadStringSet(StringSetId id, string language)
    {
        var currentLanguage = Var(UserVars.Gameplay.Language);
        language ??= currentLanguage;

        var asset = _modApplier.LoadAsset(id, language);
        return (IStringSet)asset;
    }
    string LoadStringCore(StringId id, string language)
    {
        language ??= Var(UserVars.Gameplay.Language);
        var asset = _modApplier.LoadAsset(id.Id, language);
        return asset switch
        {
            IStringSet collection => collection.GetString(id),
            string s => s,
            _ => null
        };
    }
    public bool IsStringDefined(TextId id, string language) => LoadStringCore(new StringId(id), language) != null;
    public bool IsStringDefined(StringId id, string language) => LoadStringCore(id, language) != null;
    public string LoadString(TextId id) => LoadString(new StringId(id), null);
    public string LoadString(StringId id) => LoadString(id, null);
    public string LoadString(TextId id, string language) => LoadString(new StringId(id), language);
    public string LoadString(StringId id, string language)
        => LoadStringCore(id, language) ?? $"!MISSING STRING {id.Id}:{id.SubId}!";

    public ISample LoadSample(SampleId id) => (AlbionSample)_modApplier.LoadAsset(id);
    public WaveLib LoadWaveLib(WaveLibraryId waveLibraryId) => (WaveLib)_modApplier.LoadAsset(waveLibraryId);
    public FlicFile LoadVideo(VideoId id) => (FlicFile)_modApplier.LoadAsset(id);
    public PartyMemberInfo LoadPartyMember(PartyMemberId id) => (PartyMemberInfo)_modApplier.LoadAsset(id);
    public CharacterSheet LoadSheet(SheetId id) => (CharacterSheet)_modApplier.LoadAsset(id);
    public Inventory LoadInventory(AssetId id) => (Inventory)_modApplier.LoadAsset(id);
    public IList<Block> LoadBlockList(BlockListId id) => (IList<Block>)_modApplier.LoadAsset(id);
    public EventSet LoadEventSet(EventSetId id) => (EventSet)_modApplier.LoadAsset(id);
    public byte[] LoadSong(SongId id) => (byte[]) _modApplier.LoadAsset(id);
    public IList<IEvent> LoadScript(ScriptId id) => (IList<IEvent>) _modApplier.LoadAsset(id);
    public SpellData LoadSpell(SpellId id) => (SpellData)_modApplier.LoadAsset(id);
    public SavedGame LoadSavedGame(string path) => _modApplier.LoadSavedGame(path);
    public MonsterGroup LoadMonsterGroup(MonsterGroupId id) => (MonsterGroup)_modApplier.LoadAsset(id);
    public Automap LoadAutomap(AutomapId id) => (Automap) _modApplier.LoadAsset(id);
    public object LoadSoundBanks() => _modApplier.LoadAsset(AssetId.From(Base.Special.SoundBank));
    public IVarSet LoadConfig() => (IVarSet)_modApplier.LoadAsset(AssetId.From(Base.Special.GameConfig));
    public InputConfig LoadInputConfig() => (InputConfig)_modApplier.LoadAsset(AssetId.From(Base.Special.InputConfig));
}
