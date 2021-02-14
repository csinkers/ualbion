using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UiBackground : byte
    {
        Slab = 1,
    }
}
