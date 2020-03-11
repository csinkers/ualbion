using System.Collections.Generic;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Config;

namespace UAlbion.Game
{
    public interface IAssetManager : ITextureLoader
    {
        ITexture LoadTexture(AssetType type, int id);
        ITexture LoadTexture(AutoMapId id);
        ITexture LoadTexture(CombatBackgroundId id);
        ITexture LoadTexture(CombatGraphicsId id);
        ITexture LoadTexture(DungeonBackgroundId id);
        ITexture LoadTexture(DungeonFloorId id);
        ITexture LoadTexture(DungeonObjectId id);
        ITexture LoadTexture(DungeonOverlayId id);
        ITexture LoadTexture(DungeonWallId id);
        ITexture LoadTexture(FullBodyPictureId id);
        ITexture LoadTexture(IconGraphicsId id);
        ITexture LoadTexture(ItemSpriteId id);
        ITexture LoadTexture(LargeNpcId id);
        ITexture LoadTexture(LargePartyGraphicsId id);
        ITexture LoadTexture(MonsterGraphicsId id);
        ITexture LoadTexture(PictureId id);
        ITexture LoadTexture(SmallNpcId id);
        ITexture LoadTexture(SmallPartyGraphicsId id);
        ITexture LoadTexture(SmallPortraitId id);
        ITexture LoadTexture(TacticId id);
        ITexture LoadTexture(FontId id);
        ITexture LoadTexture(MetaFontId id);
        ITexture LoadTexture(CoreSpriteId id);
        ITexture LoadFont(FontColor color, bool isBold);
        TilesetData LoadTileData(TilesetId id);
        LabyrinthData LoadLabyrinthData(LabyrinthDataId id);

        string LoadString(StringId id, GameLanguage language);
        string LoadString(SystemTextId id, GameLanguage language);
        string LoadString(WordId id, GameLanguage language);

        AlbionSample LoadSample(AssetType type, int id);
        AlbionVideo LoadVideo(VideoId id, GameLanguage language);
        AlbionPalette LoadPalette(PaletteId id);
        MapData2D LoadMap2D(MapDataId id);
        MapData3D LoadMap3D(MapDataId id);
        ItemData LoadItem(ItemId id);
        CharacterSheet LoadCharacter(AssetType type, PartyCharacterId id);
        CharacterSheet LoadCharacter(AssetType type, NpcCharacterId id);
        CharacterSheet LoadCharacter(AssetType type, MonsterCharacterId id);
        Chest LoadChest(ChestId chestId);
        Chest LoadMerchant(MerchantId merchantId);
        WordId? ParseWord(string word);
        IList<Block> LoadBlockList(BlockListId blockListId);

        IGeneralConfig LoadGeneralConfig();
        IAssetConfig LoadAssetConfig();
        CoreSpriteConfig.BinaryResource LoadCoreSpriteInfo(CoreSpriteId id);
    }
}
