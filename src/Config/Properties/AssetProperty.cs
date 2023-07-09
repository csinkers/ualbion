namespace UAlbion.Config.Properties;

public static class AssetProps
{
    // General
    public static readonly StringAssetProperty Filename = new("Filename"); // Not specified explicitly - this is loaded from the dictionary key of the file
    public static readonly StringAssetProperty Sha256Hash = new("Sha256Hash"); // Not specified explicitly - this is loaded from the dictionary key of the file
    public static readonly TypeAliasAssetProperty Container = new("Container", "container", x => x.Containers);
    public static readonly TypeAliasAssetProperty Loader = new("Loader", "loader", x => x.Loaders);
    public static readonly TypeAliasAssetProperty Post = new("Post", "post-processor", x => x.PostProcessors);

    public static readonly StringAssetProperty Language = new("Language");
    public static readonly PathPatternProperty Pattern = new("Pattern"); // Mostly for DirectoryContainer
    public static readonly BoolAssetProperty IsReadOnly = new("IsReadOnly"); // To prevent zeroing out files when repacking formats that don't have writing code yet, e.g. ILBM images
    public static readonly BoolAssetProperty UseDummyRead = new("UseDummyRead"); // For asset conversion, indicates that no asset should be loaded from the source mod, instead the target mod loader should be called directly with a dummy object
    public static readonly BoolAssetProperty Optional = new("Optional"); // Will suppress missing-asset warnings when true

    // Textures
    public static readonly IntAssetProperty Width = new("Width"); 
    public static readonly IntAssetProperty Height = new("Height"); 
    public static readonly AssetIdAssetProperty Palette = new("Palette"); // For providing context when exporting 8-bit images to true-colour PNGs
    public static readonly StringAssetProperty SubSprites = new("SubSprites"); 
    public static readonly BoolAssetProperty Transposed = new("Transposed"); // For various textures in the 3D world that are stored with rows/columns flipped
    public static readonly IntAssetProperty ExtraBytes = new("ExtraBytes"); // Used to suppress assertions when loading original assets that have incorrect sizes

    // 32-bit tileset gfx
    public static readonly StringAssetProperty DayPath = new("DayPath"); 
    public static readonly StringAssetProperty NightPath = new("NightPath"); 

    // 2D Tilesets
    public static readonly StringAssetProperty BlankTilePath = new("BlankTilePath"); 
    public static readonly PathPatternProperty GraphicsPattern = new("GraphicsPattern"); 
    public static readonly BoolAssetProperty UseSmallGraphics = new("UseSmallGraphics"); 

    // Maps

    // NPC tileset
    public static readonly BoolAssetProperty IsSmall = new("IsSmall"); //  TODO, combine with UseSmallGraphics

}

public static class AssetProperty
{
    // General
    public const string MapFile = "MapFile"; // string
    public const string Container = "Container"; // string
    public const string Loader = "Loader"; // string
    public const string Post = "Post"; // string
    public const string Language = "Language"; // string
    public const string Offset = "Offset"; // int, used for BinaryOffsetContainer, e.g. MAIN.EXE
    public const string Pattern = "Pattern"; // string, mostly for DirectoryContainer
    public const string MinimumCount = "MinimumCount"; // Just used to get closer to 1:1 round-tripping of XLDs
    public const string IsReadOnly = "IsReadOnly"; // To prevent zeroing out files when repacking formats that don't have writing code yet, e.g. ILBM images
    public const string Optional = "Optional"; // bool, will suppress missing-asset warnings when true

    // Textures
    public const string Width = "Width"; // int
    public const string Height = "Height"; // int
    public const string PaletteId = "PaletteId"; // int, for providing context when exporting 8-bit images to true-colour PNGs
    public const string SubSprites = "SubSprites"; // string
    public const string Transposed = "Transposed"; // bool, for various textures in the 3D world that are stored with rows/columns flipped
    public const string ExtraBytes = "ExtraBytes"; // int, used to suppress assertions when loading original assets that have incorrect sizes

    // Palette
    public const string IsCommon = "IsCommon"; // bool
    public const string AnimatedRanges = "AnimatedRanges"; // string (e.g. "0x1-0xf, 0x12-0x1a")

    // 32-bit tileset gfx
    public const string DayPath = "DayPath"; // string
    public const string NightPath = "NightPath"; // string

    // 2D Tilesets
    public const string BlankTilePath = "BlankTilePath"; // string
    public const string GraphicsPattern = "GraphicsPattern"; // string
    public const string UseSmallGraphics = "UseSmallGraphics"; // bool

    // Maps
    public const string LargeNpcs = "LargeNpcs"; // string
    public const string SmallNpcs = "SmallNpcs"; // string
    public const string TilesetPattern = "TilesetPattern"; // string
    public const string ScriptPattern = "ScriptPattern"; // string

    // NPC tileset
    public const string IsSmall = "IsSmall"; // bool

    // Isometric tileset/map properties
    public const string BaseHeight = "BaseHeight"; // int
    public const string CeilingPngPattern = "CeilingPngPattern"; // string
    public const string ContentsPngPattern = "ContentsPngPattern"; // string
    public const string FloorPngPattern = "FloorPngPattern"; // string
    public const string TileHeight = "TileHeight"; // int
    public const string TileWidth = "TileWidth"; // int
    public const string TiledCeilingPattern = "TiledCeilingPattern"; // string
    public const string TiledContentsPattern = "TiledContentsPattern"; // string
    public const string TiledFloorPattern = "TiledFloorPattern"; // string
    public const string TiledWallPattern = "TiledWallPattern"; // string
    public const string TilesPerRow = "TilesPerRow"; // int
    public const string WallPngPattern = "WallPngPattern"; // string

    // Spells
    public const string Name = "Name"; // StringId to use in game
    public const string MagicSchool = "School"; // SpellClass enum
    public const string SpellNumber = "SpellNumber"; // offset into school, used for save-game serialization

    // Songs
    public const string WaveLib = "WaveLib";

    // For StringSetStringLoader
    public const string Target = "Target";
    public const string FirstId = "FirstId";
}

