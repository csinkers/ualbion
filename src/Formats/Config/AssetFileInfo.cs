using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Config
{
    public abstract class AssetFileInfo
    {
        [JsonIgnore] public string Name { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(Order=-2)] public ContainerFormat ContainerFormat { get; set; }
        [JsonProperty(Order=-2)] public FileFormat Format { get; set; }
        [JsonProperty(Order=-2)] public int? Width { get; set; }
        [JsonProperty(Order=-2)] public int? Height { get; set; }
        [JsonProperty(Order=-2)] public bool? Transposed { get; set; }
        [JsonProperty(Order=-2)] public int IdOffset { get; set; }
        [JsonProperty(Order=-2)] public AssetType AssetType { get; set; }
    }

    public class BasicAssetFileInfo : AssetFileInfo
    {
        public IDictionary<int, BasicAssetInfo> Assets { get; } = new Dictionary<int, BasicAssetInfo>();
        public BasicAssetFileInfo() { }

        public BasicAssetFileInfo(FullAssetFileInfo fullAsset)
        {
            if (fullAsset == null) throw new ArgumentNullException(nameof(fullAsset));
            Name = fullAsset.Name;
            Format = fullAsset.Format;
            Width = fullAsset.Width;
            Height = fullAsset.Height;
            Transposed = fullAsset.Transposed;
        }
    }

    public class FullAssetFileInfo : AssetFileInfo
    {
        public string EnumName { get; set; }
        public string EnumType { get; set; }
        public IDictionary<int, FullAssetInfo> Assets { get; } = new Dictionary<int, FullAssetInfo>();
    }
}
