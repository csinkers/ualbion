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
        Video,
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
        Inventory,
        MerchantInventory,
        MonsterGroup,
        SpellData,
        TranslationTable,
        LabyrinthData,
        ItemData,
        ItemNames,
        SavedGame
    }
}
