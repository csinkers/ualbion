using Newtonsoft.Json;

namespace UAlbion.Config
{
    public class AssetTypeInfo
    {
        [JsonIgnore] public string Alias { get; set; }
        [JsonProperty(Order = 0)] public AssetType AssetType { get; set; }
        [JsonProperty(Order = 1)] public string EnumType { get; set; }
        [JsonProperty(Order = 2)] public string CopiedFrom { get; set; }
        public override string ToString() => $"{Alias} ({AssetType}) = {EnumType}";
    }
}
