namespace UAlbion.Config.Properties;

public static class AssetProps
{
    // General
    public static readonly StringAssetProperty Filename = new("Filename"); // Not specified explicitly - this is loaded from the dictionary key of the file
    public static readonly StringAssetProperty Sha256Hash = new("Sha256Hash"); // Not specified explicitly - this is loaded from the dictionary key of the file
    public static readonly PathPatternProperty Pattern = new("Pattern"); // Mostly for DirectoryContainer - used to specify complex file names based on the asset's id and metadata

    // How to load the asset. Container is used to pull one asset out of a file containing many, loader
    // is used to actually load the asset and the post-processor can perform any extra steps required after
    // loading (to maintain simplicity, use of post-processors should be minimised wherever possible)
    public static readonly TypeAliasAssetProperty Container = new("Container", "container", x => x.Containers);
    public static readonly TypeAliasAssetProperty Loader = new("Loader", "loader", x => x.Loaders);
    public static readonly TypeAliasAssetProperty Post = new("Post", "post-processor", x => x.PostProcessors);

    // When to load the asset. Language will cause the asset to only be loaded in that language's context,
    // IsReadOnly will cause the asset to be ignored when converting / saving, UseDummyRead will skip the
    // loading step when converting and Optional will suppress error messages about missing assets (e.g. when some languages are not available etc).
    public static readonly StringAssetProperty Language = new("Language");
    public static readonly BoolAssetProperty IsReadOnly = new("IsReadOnly"); // To prevent zeroing out files when repacking formats that don't have writing code yet, e.g. ILBM images
    public static readonly BoolAssetProperty UseDummyRead = new("UseDummyRead"); // For asset conversion, indicates that no asset should be loaded from the source mod, instead the target mod loader should be called directly with a dummy object
    public static readonly BoolAssetProperty Optional = new("Optional"); // Will suppress missing-asset warnings when true

    // Common texture-specific properties
    public static readonly IntAssetProperty Width = new("Width"); 
    public static readonly IntAssetProperty Height = new("Height"); 
    public static readonly AssetIdAssetProperty Palette = new("Palette"); // For providing context when exporting 8-bit images to true-colour PNGs and reimporting the true-colour PNGs back to 8-bit images.
}

