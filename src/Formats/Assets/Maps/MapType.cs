using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets.Maps
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MapType : byte
    {
        Unknown = 0,
        ThreeD = 1,
        TwoD = 2,
        TwoDOutdoors = 3,
    }
}
