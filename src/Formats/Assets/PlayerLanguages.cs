using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlayerLanguages : byte
    {
        None = 0,
        Terran = 1,
        Iskai = 2,
        Celtic = 4
    }
}
