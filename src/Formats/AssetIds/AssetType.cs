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
            AssetType.AssetConfig          => "AConf",
            AssetType.CoreSpriteConfig     => "CConf",
            AssetType.CoreGraphicsMetadata => "CorGM",
            AssetType.GeneralConfig        => "GConf",
            AssetType.MetaFont             => "MetaF",
            AssetType.PaletteNull          => "Pal00",
            AssetType.SavedGame            => "SaveG",
            AssetType.SoundBank            => "SndBk",
            AssetType.Slab                 => "Slab0",
            AssetType.Unnamed2             => "Unk02",
            AssetType.Automap              => "AutoM",
            AssetType.AutomapGraphics      => "AutoG",
            AssetType.BlockList            => "BlckL",
            AssetType.ChestData            => "Chest",
            AssetType.CombatBackground     => "ComBg",
            AssetType.CombatGraphics       => "ComGx",
            AssetType.CoreGraphics         => "CorGx",
            AssetType.BackgroundGraphics   => "BgGfx",
            AssetType.Floor3D              => "Floor",
            AssetType.Object3D             => "Obj3d",
            AssetType.Overlay3D            => "Ovrly",
            AssetType.Wall3D               => "Wall3",
            AssetType.EventSet             => "EvntS",
            AssetType.EventText            => "EvntT",
            AssetType.Font                 => "Fonts",
            AssetType.FullBodyPicture      => "FBPic",
            AssetType.Tileset              => "TileS",
            AssetType.IconGraphics         => "TileG",
            AssetType.ItemGraphics         => "ItemG",
            AssetType.ItemList             => "ItemL",
            AssetType.ItemNames            => "ItemN",
            AssetType.LabData              => "Labyr",
            AssetType.BigNpcGraphics       => "BgNpc",
            AssetType.BigPartyGraphics     => "BgPrt",
            AssetType.MapData              => "MapDt",
            AssetType.MapText              => "MapTx",
            AssetType.MerchantData         => "Merch",
            AssetType.Monster              => "Monst",
            AssetType.MonsterGraphics      => "MonGx",
            AssetType.MonsterGroup         => "MonGp",
            AssetType.Npc                  => "NpcDt",
            AssetType.Palette              => "Palet",
            AssetType.PartyMember          => "Party",
            AssetType.Picture              => "Pictr",
            AssetType.Sample               => "Sampl",
            AssetType.Script               => "Scrpt",
            AssetType.SmallNpcGraphics     => "SmNpc",
            AssetType.SmallPartyGraphics   => "SmPrt",
            AssetType.SmallPortrait        => "Prtrt",
            AssetType.Song                 => "Songs",
            AssetType.SpellData            => "Spell",
            AssetType.SystemText           => "SText",
            AssetType.TacticalIcon         => "TaIco",
            AssetType.Flic                 => "FlicV",
            AssetType.WaveLibrary          => "WaveL",
            AssetType.Dictionary           => "Words",
            AssetType.UAlbionText          => "UText",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public static AssetType FromShort(string shortName) => shortName switch
        {
            "AConf" => AssetType.AssetConfig,
            "CConf" => AssetType.CoreSpriteConfig,
            "CorGM" => AssetType.CoreGraphicsMetadata,
            "GConf" => AssetType.GeneralConfig,
            "MetaF" => AssetType.MetaFont,
            "Pal00" => AssetType.PaletteNull,
            "SaveG" => AssetType.SavedGame,
            "SndBk" => AssetType.SoundBank,
            "Slab0" => AssetType.Slab,
            "Unk02" => AssetType.Unnamed2,
            "AutoM" => AssetType.Automap,
            "AutoG" => AssetType.AutomapGraphics,
            "BlckL" => AssetType.BlockList,
            "Chest" => AssetType.ChestData,
            "ComBg" => AssetType.CombatBackground,
            "ComGx" => AssetType.CombatGraphics,
            "CorGx" => AssetType.CoreGraphics,
            "BgGfx" => AssetType.BackgroundGraphics,
            "Floor" => AssetType.Floor3D,
            "Obj3d" => AssetType.Object3D,
            "Ovrly" => AssetType.Overlay3D,
            "Wall3" => AssetType.Wall3D,
            "EvntS" => AssetType.EventSet,
            "EvntT" => AssetType.EventText,
            "Fonts" => AssetType.Font,
            "FBPic" => AssetType.FullBodyPicture,
            "TileS" => AssetType.Tileset,
            "TileG" => AssetType.IconGraphics,
            "ItemG" => AssetType.ItemGraphics,
            "ItemL" => AssetType.ItemList,
            "ItemN" => AssetType.ItemNames,
            "Labyr" => AssetType.LabData,
            "BgNpc" => AssetType.BigNpcGraphics,
            "BgPrt" => AssetType.BigPartyGraphics,
            "MapDt" => AssetType.MapData,
            "MapTx" => AssetType.MapText,
            "Merch" => AssetType.MerchantData,
            "Monst" => AssetType.Monster,
            "MonGx" => AssetType.MonsterGraphics,
            "MonGp" => AssetType.MonsterGroup,
            "NpcDt" => AssetType.Npc,
            "Palet" => AssetType.Palette,
            "Party" => AssetType.PartyMember,
            "Pictr" => AssetType.Picture,
            "Sampl" => AssetType.Sample,
            "Scrpt" => AssetType.Script,
            "SmNpc" => AssetType.SmallNpcGraphics,
            "SmPrt" => AssetType.SmallPartyGraphics,
            "Prtrt" => AssetType.SmallPortrait,
            "Songs" => AssetType.Song,
            "Spell" => AssetType.SpellData,
            "SText" => AssetType.SystemText,
            "TaIco" => AssetType.TacticalIcon,
            "FlicV" => AssetType.Flic,
            "WaveL" => AssetType.WaveLibrary,
            "Words" => AssetType.Dictionary,
            "UText" => AssetType.UAlbionText,
            _ => throw new ArgumentOutOfRangeException(nameof(shortName), shortName, null)
        };
    }
}
