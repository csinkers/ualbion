namespace UAlbion.Formats.Config
{
    public enum XldObjectType
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

        // Maps
        MapData,
        IconData,

        // Audio
        AudioSample,
        SampleLibrary,
        Song,

        // Text
        StringTable,
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
    }
}