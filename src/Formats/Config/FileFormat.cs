namespace UAlbion.Formats.Config
{
    public enum FileFormat
    {
        Unknown,

        // Graphics
        AmorphousSprite,
        FixedSizeSprite,
        HeaderPerSubImageSprite,
        InterlacedBitmap,
        Palette,
        PaletteCommon,
        SingleHeaderSprite,
        FlicVideo,
        Font,
        Slab, // Just a FixedSizeSprite, but custom loader to add a subimage for the status bar

        // Maps
        MapData,
        Tileset,

        // Audio
        AudioSample,
        SampleLibrary,
        Song,

        // Text
        StringTable,
        SystemText,
        Script,
        WordList,

        // Misc
        BlockList,
        CharacterData,
        EventSet,
        PlayerInventory,
        ChestInventory,
        MerchantInventory,
        MonsterGroup,
        SpellData,
        LabyrinthData,
        ItemData,
        ItemNames,
        SavedGame
    }
}
