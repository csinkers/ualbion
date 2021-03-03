using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game
{
    public interface IAssetManager : ITextureLoader
    {
        AssetInfo GetAssetInfo(AssetId id, string language = null);
        ITexture LoadTexture(SpriteId id);
        ITexture LoadFont(FontColor color, bool isBold);
        TilesetData LoadTileData(TilesetId id);
        LabyrinthData LoadLabyrinthData(LabyrinthId id);
        bool IsStringDefined(TextId id, string language);
        bool IsStringDefined(StringId id, string language);
        string LoadString(TextId id);
        string LoadString(StringId id);
        ISample LoadSample(SampleId id);
        WaveLib LoadWaveLib(WaveLibraryId waveLibraryId);
        FlicFile LoadVideo(VideoId id);
        AlbionPalette LoadPalette(PaletteId id);
        IMapData LoadMap(MapId id);
        ItemData LoadItem(ItemId id);
        CharacterSheet LoadSheet(CharacterId id);
        Inventory LoadInventory(AssetId id); // TODO: Use InventoryId?
        IList<Block> LoadBlockList(BlockListId id);
        EventSet LoadEventSet(EventSetId id);
        byte[] LoadSong(SongId id);
        IList<IEvent> LoadScript(ScriptId id);
        SpellData LoadSpell(SpellId id);
        SavedGame LoadSavedGame(string path);
        MonsterGroup LoadMonsterGroup(MonsterGroupId id);
        Automap LoadAutomap(AutomapId id);
        byte[] LoadSoundBanks(); // Special assets
    }
}
