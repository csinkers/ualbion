using System;

namespace UAlbion.Formats.AssetIds
{
    public class EnumTypeAttribute : Attribute
    {
        public Type EnumType { get; }
        public EnumTypeAttribute(Type enumType)
        {
            EnumType = enumType;
        }
    }

    public enum AssetType
    {
        CoreGraphicsMetadata,
        MetaFont,
        PaletteNull,
        Slab,
        Unnamed2, // Unused
        [EnumType(typeof(AutoMapId))] Automap,
        [EnumType(typeof(AutoMapId))] AutomapGraphics,
        [EnumType(typeof(BlockListId))] BlockList,
        [EnumType(typeof(ChestId))] ChestData,
        [EnumType(typeof(CombatBackgroundId))] CombatBackground,
        [EnumType(typeof(CombatGraphicsId))] CombatGraphics,
        [EnumType(typeof(CoreSpriteId))] CoreGraphics,
        [EnumType(typeof(DungeonBackgroundId))] BackgroundGraphics,
        [EnumType(typeof(DungeonFloorId))] Floor3D,
        [EnumType(typeof(DungeonObjectId))] Object3D,
        [EnumType(typeof(DungeonOverlayId))] Overlay3D,
        [EnumType(typeof(DungeonWallId))] Wall3D,
        [EnumType(typeof(EventSetId))] EventSet,
        [EnumType(typeof(EventTextId))] EventText,
        [EnumType(typeof(FontId))] Font,
        [EnumType(typeof(FullBodyPictureId))] FullBodyPicture,
        [EnumType(typeof(IconDataId))] IconData,
        [EnumType(typeof(IconGraphicsId))] IconGraphics,
        [EnumType(typeof(ItemId))] ItemGraphics,
        [EnumType(typeof(ItemId))] ItemList,
        [EnumType(typeof(ItemId))] ItemNames,
        [EnumType(typeof(LabyrinthDataId))] LabData,
        [EnumType(typeof(LargeNpcId))] BigNpcGraphics,
        [EnumType(typeof(LargePartyGraphicsId))] BigPartyGraphics,
        [EnumType(typeof(MapDataId))] MapData,
        [EnumType(typeof(MapTextId))] MapText,
        [EnumType(typeof(MerchantId))] MerchantData,
        [EnumType(typeof(MonsterCharacterId))] Monster,
        [EnumType(typeof(MonsterGraphicsId))] MonsterGraphics,
        [EnumType(typeof(MonsterGroupId))]MonsterGroup,
        [EnumType(typeof(NpcCharacterId))] Npc,
        [EnumType(typeof(PaletteId))] Palette,
        [EnumType(typeof(PartyCharacterId))] PartyMember,
        [EnumType(typeof(PictureId))] Picture,
        [EnumType(typeof(SampleId))] Sample,
        [EnumType(typeof(ScriptId))] Script,
        [EnumType(typeof(SmallNpcId))] SmallNpcGraphics,
        [EnumType(typeof(SmallPartyGraphicsId))] SmallPartyGraphics,
        [EnumType(typeof(SmallPortraitId))] SmallPortrait,
        [EnumType(typeof(SongId))] Song,
        [EnumType(typeof(SpellId))] SpellData,
        [EnumType(typeof(SystemTextId))] SystemText,
        [EnumType(typeof(TacticId))] TacticalIcon,
        [EnumType(typeof(TranslationTableId))] TransparencyTables,
        [EnumType(typeof(VideoId))] Flic,
        [EnumType(typeof(WaveLibraryId))] WaveLibrary,
        [EnumType(typeof(WordId))] Dictionary,
    }

}
