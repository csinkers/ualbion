using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats;

public interface IAssetManager : ITextureLoader
{
    AssetInfo GetAssetInfo(AssetId id, string language = null);
    ITexture LoadTexture(SpriteId id);
    ITileGraphics LoadTileGraphics(TilesetGfxId id);
    object LoadMapObject(MapObjectId id); // Might be an ITexture or a Mesh
    Ink LoadInk(InkId id);
    FontDefinition LoadFontDefinition(FontId id);
    MetaFont LoadFont(FontId fontId, InkId inkId);
    TilesetData LoadTileData(TilesetId id);
    LabyrinthData LoadLabyrinthData(LabyrinthId id);
    bool IsStringDefined(TextId id, string language);
    bool IsStringDefined(StringId id, string language);
    string LoadString(TextId id);
    string LoadString(StringId id);
    string LoadString(TextId id, string language);
    string LoadString(StringId id, string language);
    ISample LoadSample(SampleId id);
    WaveLib LoadWaveLib(WaveLibraryId waveLibraryId);
    FlicFile LoadVideo(VideoId id);
    AlbionPalette LoadPalette(PaletteId id);
    IMapData LoadMap(MapId id);
    ItemData LoadItem(ItemId id);
    PartyMemberInfo LoadPartyMember(PartyMemberId id);
    CharacterSheet LoadSheet(SheetId id);
    Inventory LoadInventory(AssetId id); // TODO: Use InventoryId?
    IList<Block> LoadBlockList(BlockListId id);
    EventSet LoadEventSet(EventSetId id);
    byte[] LoadSong(SongId id);
    IList<IEvent> LoadScript(ScriptId id);
    SpellData LoadSpell(SpellId id);
    SavedGame LoadSavedGame(string path);
    MonsterGroup LoadMonsterGroup(MonsterGroupId id);
    Automap LoadAutomap(AutomapId id);
    object LoadSoundBanks(); // Should always return a GlobalTimbreLibrary, but we don't want to force a dependency on ADLMidi.NET in UAlbion.Formats, so use object
    IVarSet LoadConfig();
    InputConfig LoadInputConfig();
}

public static class AssetManagerExtensions
{
    public static ItemData LoadItemStrict(this IAssetManager assets, ItemId id)
    {
        if (id.Type != AssetType.Item)
            throw new ArgumentOutOfRangeException(nameof(id), id, $"Tried to get item for {id}, but it is not an item id");

        var item = assets.LoadItem(id);
        if (item == null)
            throw new AssetNotFoundException($"Could not find item data for {id}", id);

        return item;
    }
}