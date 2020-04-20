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
        AssetConfig,
        CoreSpriteConfig,
        CoreGraphicsMetadata,
        GeneralConfig,
        MetaFont,
        PaletteNull,
        SoundBank,
        Slab, // Graphics
        Unnamed2, // Unused
        [EnumType(typeof(AutoMapId))]            Automap,            // Map
        [EnumType(typeof(AutoMapId))]            AutomapGraphics,    // Graphics
        [EnumType(typeof(BlockListId))]          BlockList,          // Map
        [EnumType(typeof(ChestId))]              ChestData,          // Inventory
        [EnumType(typeof(CombatBackgroundId))]   CombatBackground,   // Graphics
        [EnumType(typeof(CombatGraphicsId))]     CombatGraphics,     // Graphics
        [EnumType(typeof(CoreSpriteId))]         CoreGraphics,       // Graphics
        [EnumType(typeof(DungeonBackgroundId))]  BackgroundGraphics, // Graphics
        [EnumType(typeof(DungeonFloorId))]       Floor3D,            // Graphics
        [EnumType(typeof(DungeonObjectId))]      Object3D,           // Graphics
        [EnumType(typeof(DungeonOverlayId))]     Overlay3D,          // Graphics
        [EnumType(typeof(DungeonWallId))]        Wall3D,             // Graphics
        [EnumType(typeof(EventSetId))]           EventSet,           // Events
        [EnumType(typeof(EventTextId))]          EventText,          // Text
        [EnumType(typeof(FontId))]               Font,               // Graphics
        [EnumType(typeof(FullBodyPictureId))]    FullBodyPicture,    // Graphics
        [EnumType(typeof(TilesetId))]            Tileset,            // Map
        [EnumType(typeof(IconGraphicsId))]       IconGraphics,       // Graphics
        [EnumType(typeof(ItemId))]               ItemGraphics,       // Graphics
        [EnumType(typeof(ItemId))]               ItemList,           // Misc
        [EnumType(typeof(ItemId))]               ItemNames,          // Text
        [EnumType(typeof(LabyrinthDataId))]      LabData,            // Map
        [EnumType(typeof(LargeNpcId))]           BigNpcGraphics,     // Graphics
        [EnumType(typeof(LargePartyGraphicsId))] BigPartyGraphics,   // Graphics
        [EnumType(typeof(MapDataId))]            MapData,            // Map
        [EnumType(typeof(MapTextId))]            MapText,            // Text
        [EnumType(typeof(MerchantId))]           MerchantData,       // Inventory
        [EnumType(typeof(MonsterCharacterId))]   Monster,            // CharacterSheets
        [EnumType(typeof(MonsterGraphicsId))]    MonsterGraphics,    // Graphics
        [EnumType(typeof(MonsterGroupId))]       MonsterGroup,       // Misc
        [EnumType(typeof(NpcCharacterId))]       Npc,                // CharacterSheets
        [EnumType(typeof(PaletteId))]            Palette,            // Graphics
        [EnumType(typeof(PartyCharacterId))]     PartyMember,        // CharacterSheets
        [EnumType(typeof(PictureId))]            Picture,            // Graphics
        [EnumType(typeof(SampleId))]             Sample,             // Audio
        [EnumType(typeof(ScriptId))]             Script,             // Events
        [EnumType(typeof(SmallNpcId))]           SmallNpcGraphics,   // Graphics
        [EnumType(typeof(SmallPartyGraphicsId))] SmallPartyGraphics, // Graphics
        [EnumType(typeof(SmallPortraitId))]      SmallPortrait,      // Graphics
        [EnumType(typeof(SongId))]               Song,               // Audio
        [EnumType(typeof(SpellId))]              SpellData,          // Misc
        [EnumType(typeof(SystemTextId))]         SystemText,         // Text
        [EnumType(typeof(TacticId))]             TacticalIcon,       // Graphics
        [EnumType(typeof(TranslationTableId))]   TransparencyTables, // Misc
        [EnumType(typeof(VideoId))]              Flic,               // Graphics
        [EnumType(typeof(WaveLibraryId))]        WaveLibrary,        // Audio
        [EnumType(typeof(WordId))]               Dictionary,         // Text
        [EnumType(typeof(UAlbionStringId))]      UAlbionText,        // JSON text
    }
}
