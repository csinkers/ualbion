using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace UAlbion.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ItemGraphics : byte
    {
        ItemSprites = 1,
    }
}
