using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace UAlbion.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Font : byte
    {
        RegularFont = 1,
        BoldFont = 2,
    }
}
