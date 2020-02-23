using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Config
{
    public abstract class XldInfo
    {
        [JsonIgnore] public string Name;

        [JsonConverter(typeof(StringEnumConverter))]
        public FileFormat Format;
        public int? Width;
        public int? Height;
        public bool? Transposed;
    }

    public class BasicXldInfo : XldInfo
    {
        public IDictionary<int, BasicAssetInfo> Assets { get; } = new Dictionary<int, BasicAssetInfo>();
        public BasicXldInfo() { }

        public BasicXldInfo(FullXldInfo full)
        {
            Name = full.Name;
            Format = full.Format;
            Width = full.Width;
            Height = full.Height;
            Transposed = full.Transposed;
        }
    }

    public class FullXldInfo : XldInfo
    {
        public string EnumName { get; set; }
        public string EnumType { get; set; }
        public IDictionary<int, FullAssetInfo> Assets { get; } = new Dictionary<int, FullAssetInfo>();
    }
}