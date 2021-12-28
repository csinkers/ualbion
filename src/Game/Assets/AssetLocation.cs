namespace UAlbion.Game.Assets;

enum AssetLocation
{
    /// <summary>
    /// For assets that are stored in XLD files in the base XLDLIBS directory, and that do not vary by language.
    /// </summary>
    Base,
    /// <summary>
    /// For assets that are stored in non-XLD files / some form of raw format in the base XLDLIBS directory, and that do not vary by language.
    /// </summary>
    BaseRaw,
    /// <summary>
    /// For assets that are stored in XLD files in language specific subdirectories of XLDLIBS.
    /// </summary>
    Localised,
    /// <summary>
    /// For assets that are stored in non-XLD files in language specific subdirectories of XLDLIBS.
    /// </summary>
    LocalisedRaw,
    /// <summary>
    /// For assets that change through the course of a game. Loaded from XLDLIBS/INITIAL when a new game starts, otherwise loaded from save files.
    /// </summary>
    Initial,
    /// <summary>
    /// Used by the original game to temporarily store the current version of INITIAL files when a game is in progress, this remake will just keep the data in memory instead.
    /// </summary>
    Current,
    /// <summary>
    /// For assets that are baked into the actual MAIN.EXE of the original game.
    /// </summary>
    MainExe,
    /// <summary>
    /// Metadata and other resources with special handling
    /// </summary>
    Meta
}