using System.Text.Json.Serialization;

namespace UAlbion.Config;

public class AssetTypeInfo
{
    [JsonIgnore] public string Alias { get; set; }
    public AssetType AssetType { get; set; }
    public string EnumType { get; set; }
    public override string ToString() => $"{Alias} ({AssetType}) = {EnumType}";
}