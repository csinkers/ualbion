using UAlbion.Api;
using UAlbion.Config.Properties;

namespace UAlbion.Config;

public record AssetLoadContext(
    AssetId AssetId,
    AssetNode Node,
    ModContext ModContext,
    string Language = null)
{
    public T GetProperty<T>(IAssetProperty<T> assetProperty)
        => Node.GetProperty(assetProperty);
    public T GetProperty<T>(IAssetProperty<T> assetProperty, T defaultValue)
        => Node.GetProperty(assetProperty, defaultValue);
    public void SetProperty<T>(IAssetProperty<T> assetProperty, T value)
        => Node.SetProperty(assetProperty, value);

    public int Index => Node.GetIndex(AssetId);
    public string Filename => Node.Filename;
    public string Sha256Hash => Node.Sha256Hash;
    public string ModName => ModContext.ModName;
    public IJsonUtil Json => ModContext.Json;
    public IFileSystem Disk => ModContext.Disk;
    public AssetMapping Mapping => ModContext.Mapping;
    public AssetPath BuildAssetPath(int subAsset = 0, string overrideName = null)
        => new(AssetId, subAsset, Node.GetProperty(AssetProps.Palette).Id, overrideName);

    // Common loader / container specific properties
    public int Width => Node.Width; // The default width, in pixels, of frames in images inside the file. For sprites only.
    public int Height => Node.Height; // The default height, in pixels, of frames in images inside the file. For sprites only.
    public AssetId PaletteId => Node.PaletteId; //for providing context when exporting 8-bit images to true-colour PNGs

    public override string ToString()
    {
        var hashPart = (string.IsNullOrEmpty(Sha256Hash) ? "" : $"#{Sha256Hash}");
        return $"I:{AssetId} ({Filename}{hashPart}.{Index})";
    }
}