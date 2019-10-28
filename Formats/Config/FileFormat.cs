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

        // Maps
        MapData,
        IconData,

        // Audio
        AudioSample,
        SampleLibrary,
        Song,

        // Text
        StringTable,
        SystemText,
        Script,

        // Misc
        BlockList,
        CharacterData,
        EventSet,
        Inventory,
        MonsterGroup,
        SpellData,
        TranslationTable,
        LabyrinthData,
        ItemData,
        ItemNames
    }
}