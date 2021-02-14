using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace UAlbion.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AutomapTiles : byte
    {
        Set1 = 1,
        Set2 = 2,
    }
}
