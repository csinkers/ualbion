using System;

namespace UAlbion.Formats.AssetIds
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumTypeAttribute : Attribute
    {
        public Type EnumType { get; }
        public EnumTypeAttribute(Type enumType)
        {
            EnumType = enumType;
        }
    }

    public enum AssetType : byte
    {
        AssetConfig,
        CoreSpriteConfig,
        CoreGraphicsMetadata,
        GeneralConfig,
        MetaFont,
        PaletteNull,
        SavedGame,
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
        [EnumType(typeof(ItemSpriteId))]         ItemGraphics,       // Graphics
        [EnumType(typeof(ItemId))]               ItemList,           // Misc
                                                 ItemNames,          // Text
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
        [EnumType(typeof(VideoId))]              Flic,               // Graphics
        [EnumType(typeof(WaveLibraryId))]        WaveLibrary,        // Audio
        [EnumType(typeof(WordId))]               Dictionary,         // Text
        [EnumType(typeof(UAlbionStringId))]      UAlbionText,        // JSON text
    }

    public static class AssetTypeExtensions
    {
        public static string ToShortName(this AssetType type) => type switch
        {
            AssetType.AssetConfig          => "AssetCfg",
            AssetType.CoreSpriteConfig     => "CoreCfg",
            AssetType.CoreGraphicsMetadata => "CoreGfxMeta",
            AssetType.GeneralConfig        => "GenCfg",
            AssetType.MetaFont             => "MetaFont",
            AssetType.PaletteNull          => "PAL00",
            AssetType.SavedGame            => "SaveGame",
            AssetType.SoundBank            => "SBank",
            AssetType.Slab                 => "Slab",
            AssetType.Unnamed2             => "Unk02",
            AssetType.Automap              => "AMap",
            AssetType.AutomapGraphics      => "AMapGfx",
            AssetType.BlockList            => "Blocks",
            AssetType.ChestData            => "Chest",
            AssetType.CombatBackground     => "ComBG",
            AssetType.CombatGraphics       => "ComGfx",
            AssetType.CoreGraphics         => "CorGfx",
            AssetType.BackgroundGraphics   => "BgGfx",
            AssetType.Floor3D              => "Floor",
            AssetType.Object3D             => "Obj",
            AssetType.Overlay3D            => "Overlay",
            AssetType.Wall3D               => "Wall",
            AssetType.EventSet             => "ESet",
            AssetType.EventText            => "ETxt",
            AssetType.Font                 => "Font",
            AssetType.FullBodyPicture      => "BodyPic",
            AssetType.Tileset              => "TileSet",
            AssetType.IconGraphics         => "TileGfx",
            AssetType.ItemGraphics         => "ItemGfx",
            AssetType.ItemList             => "ItemList",
            AssetType.ItemNames            => "ItemNames",
            AssetType.LabData              => "LabData",
            AssetType.BigNpcGraphics       => "BigNpc",
            AssetType.BigPartyGraphics     => "BigParty",
            AssetType.MapData              => "Map",
            AssetType.MapText              => "MapTxt",
            AssetType.MerchantData         => "Merch",
            AssetType.Monster              => "Monst",
            AssetType.MonsterGraphics      => "MonGx",
            AssetType.MonsterGroup         => "MonGrp",
            AssetType.Npc                  => "Npc",
            AssetType.Palette              => "PAL",
            AssetType.PartyMember          => "Party",
            AssetType.Picture              => "Pic",
            AssetType.Sample               => "Sample",
            AssetType.Script               => "Script",
            AssetType.SmallNpcGraphics     => "SmlNpc",
            AssetType.SmallPartyGraphics   => "SmlParty",
            AssetType.SmallPortrait        => "Portrait",
            AssetType.Song                 => "Songs",
            AssetType.SpellData            => "Spell",
            AssetType.SystemText           => "SysTxt",
            AssetType.TacticalIcon         => "TacIcon",
            AssetType.Flic                 => "Flic",
            AssetType.WaveLibrary          => "WaveLib",
            AssetType.Dictionary           => "Words",
            AssetType.UAlbionText          => "UText",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public static AssetType FromShort(string shortName) => shortName switch
        {
            "AssetCfg"    => AssetType.AssetConfig,
            "CoreCfg"     => AssetType.CoreSpriteConfig,
            "CoreGfxMeta" => AssetType.CoreGraphicsMetadata,
            "GenCfg"      => AssetType.GeneralConfig,
            "MetaFont"    => AssetType.MetaFont,
            "PAL00"       => AssetType.PaletteNull,
            "SaveGame"    => AssetType.SavedGame,
            "SBank"       => AssetType.SoundBank,
            "Slab"        => AssetType.Slab,
            "Unk02"       => AssetType.Unnamed2,
            "AMap"        => AssetType.Automap,
            "AMapGfx"     => AssetType.AutomapGraphics,
            "Blocks"      => AssetType.BlockList,
            "Chest"       => AssetType.ChestData,
            "ComBG"       => AssetType.CombatBackground,
            "ComGfx"      => AssetType.CombatGraphics,
            "CorGfx"      => AssetType.CoreGraphics,
            "BgGfx"       => AssetType.BackgroundGraphics,
            "Floor"       => AssetType.Floor3D,
            "Obj"         => AssetType.Object3D,
            "Overlay"     => AssetType.Overlay3D,
            "Wall"        => AssetType.Wall3D,
            "ESet"        => AssetType.EventSet,
            "ETxt"        => AssetType.EventText,
            "Font"        => AssetType.Font,
            "BodyPic"     => AssetType.FullBodyPicture,
            "TileSet"     => AssetType.Tileset,
            "TileGfx"     => AssetType.IconGraphics,
            "ItemGfx"     => AssetType.ItemGraphics,
            "ItemList"    => AssetType.ItemList,
            "ItemNames"   => AssetType.ItemNames,
            "LabData"     => AssetType.LabData,
            "BigNpc"      => AssetType.BigNpcGraphics,
            "BigParty"    => AssetType.BigPartyGraphics,
            "Map"         => AssetType.MapData,
            "MapTxt"      => AssetType.MapText,
            "Merch"       => AssetType.MerchantData,
            "Monst"       => AssetType.Monster,
            "MonGx"       => AssetType.MonsterGraphics,
            "MonGrp"      => AssetType.MonsterGroup,
            "Npc"         => AssetType.Npc,
            "PAL"         => AssetType.Palette,
            "Party"       => AssetType.PartyMember,
            "Pic"         => AssetType.Picture,
            "Sample"      => AssetType.Sample,
            "Script"      => AssetType.Script,
            "SmlNpc"      => AssetType.SmallNpcGraphics,
            "SmlParty"    => AssetType.SmallPartyGraphics,
            "Portrait"    => AssetType.SmallPortrait,
            "Songs"       => AssetType.Song,
            "Spell"       => AssetType.SpellData,
            "SysTxt"      => AssetType.SystemText,
            "TacIcon"     => AssetType.TacticalIcon,
            "Flic"        => AssetType.Flic,
            "WaveLib"     => AssetType.WaveLibrary,
            "Words"       => AssetType.Dictionary,
            "UText"       => AssetType.UAlbionText,
            _ => throw new ArgumentOutOfRangeException(nameof(shortName), shortName, null)
        };
    }
}
