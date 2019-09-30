using UAlbion.Core.Textures;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game
{
    public interface IAssetManager
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
        ITexture LoadTexture(IconDataId id);
        ITexture LoadTexture(IconGraphicsId id);
        ITexture LoadTexture(ItemId id);
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
        ITexture LoadFont(MetaFontId.FontColor color, bool isBold);
        TilesetData LoadTileData(IconDataId id);
        LabyrinthData LoadLabyrinthData(LabyrinthDataId id);
        CoreSpriteConfig.BinaryResource LoadCoreSpriteInfo(CoreSpriteId id);

        string LoadString(AssetType type, int id, GameLanguage language, int subItem);
        string LoadString(SystemTextId id, GameLanguage language);
        string LoadString(WordId id, GameLanguage language);

        AlbionSample LoadSample(AssetType type, int id);
        AlbionVideo LoadVideo(VideoId id, GameLanguage language);
        AlbionPalette LoadPalette(PaletteId id);
        MapData2D LoadMap2D(MapDataId id);
        MapData3D LoadMap3D(MapDataId id);
    }
}