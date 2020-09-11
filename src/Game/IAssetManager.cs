using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;

namespace UAlbion.Game
{
    public interface IAssetManager : ITextureLoader
    {
        AssetInfo GetAssetInfo(AssetKey key);
        ITexture LoadFont(FontColor color, bool isBold);
        TilesetData LoadTileData(TilesetId id);
        LabyrinthData LoadLabyrinthData(LabyrinthDataId id);
        string LoadString(StringId id, GameLanguage language);
        ISample LoadSample(SampleId id);
        ISample LoadWaveLib(SongId songId, int instrument);
        byte[] LoadSoundBanks();
        FlicFile LoadVideo(VideoId id, GameLanguage language);
        AlbionPalette LoadPalette(PaletteId id);
        IMapData LoadMap(MapDataId id);
        ItemData LoadItem(ItemId id);
        CharacterSheet LoadPartyMember(PartyCharacterId id);
        CharacterSheet LoadNpc(NpcCharacterId id);
        CharacterSheet LoadMonster(MonsterCharacterId id);
        Inventory LoadChest(ChestId chestId);
        Inventory LoadMerchant(MerchantId merchantId);
        WordId? ParseWord(string word);
        IList<Block> LoadBlockList(BlockListId blockListId);
        IGeneralConfig LoadGeneralConfig();
        IAssetConfig LoadAssetConfig();
        CoreSpriteInfo LoadCoreSpriteInfo(CoreSpriteId id);
        EventSet LoadEventSet(EventSetId eventSetId);
        byte[] LoadSong(SongId songId);
        IList<IEvent> LoadScript(ScriptId scriptId);
        SpellData LoadSpell(SpellId spellId);
        SavedGame LoadSavedGame(ushort id);
        MonsterGroup LoadMonsterGroup(MonsterGroupId groupId);
    }
}
