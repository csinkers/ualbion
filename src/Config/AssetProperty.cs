namespace UAlbion.Config;

public static class AssetProperty
{
    // General
    public const string Language = "Language"; // string
    public const string Offset   = "Offset"; // int, used for BinaryOffsetContainer, e.g. MAIN.EXE
    public const string Pattern  = "Pattern"; // string, mostly for DirectoryContainer
    public const string MinimumCount = "MinimumCount"; // Just used to get closer to 1:1 round-tripping of XLDs
    public const string IsReadOnly = "IsReadOnly"; // To prevent zeroing out files when repacking formats that don't have writing code yet, e.g. ILBM images
    public static string Optional = "Optional"; // bool, will suppress missing-asset warnings when true

    // Textures
    public const string Width      = "Width"; // int
    public const string Height     = "Height"; // int
    public const string PaletteId  = "PaletteId"; // int, for providing context when exporting 8-bit images to true-colour PNGs
    public const string SubSprites = "SubSprites"; // string
    public const string Transposed = "Transposed"; // bool, for various textures in the 3D world that are stored with rows/columns flipped
    public const string ExtraBytes = "ExtraBytes"; // int, used to suppress assertions when loading original assets that have incorrect sizes

    // Palette
    public const string IsCommon       = "IsCommon"; // bool
    public const string AnimatedRanges = "AnimatedRanges"; // string (e.g. "0x1-0xf, 0x12-0x1a")

    // 32-bit tileset gfx
    public const string DayPath   = "DayPath"; // string
    public const string NightPath = "NightPath"; // string

    // 2D Tilesets
    public const string BlankTilePath    = "BlankTilePath"; // string
    public const string GraphicsPattern  = "GraphicsPattern"; // string
    public const string UseSmallGraphics = "UseSmallGraphics"; // bool

    // Maps
    public const string LargeNpcs      = "LargeNpcs"; // string
    public const string SmallNpcs      = "SmallNpcs"; // string
    public const string TilesetPattern = "TilesetPattern"; // string
    public const string ScriptPattern  = "ScriptPattern"; // string

    // NPC tileset
    public const string IsSmall = "IsSmall"; // bool

    // Isometric tileset/map properties
    public const string BaseHeight           = "BaseHeight"; // int
    public const string CeilingPngPattern    = "CeilingPngPattern"; // string
    public const string ContentsPngPattern   = "ContentsPngPattern"; // string
    public const string FloorPngPattern      = "FloorPngPattern"; // string
    public const string TileHeight           = "TileHeight"; // int
    public const string TileWidth            = "TileWidth"; // int
    public const string TiledCeilingPattern  = "TiledCeilingPattern"; // string
    public const string TiledContentsPattern = "TiledContentsPattern"; // string
    public const string TiledFloorPattern    = "TiledFloorPattern"; // string
    public const string TiledWallPattern     = "TiledWallPattern"; // string
    public const string TilesPerRow          = "TilesPerRow"; // int
    public const string WallPngPattern       = "WallPngPattern"; // string

    // Spells
    public const string Name = "Name"; // StringId to use in game
    public const string MagicSchool = "School"; // SpellClass enum
    public const string SpellNumber = "SpellNumber"; // offset into school, used for save-game serialization

    // Songs
    public const string WaveLib = "WaveLib";
}