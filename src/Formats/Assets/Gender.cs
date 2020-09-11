using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Gender : byte
    {
        Male = 0,
        Female = 1,
        Neuter = 2,
    }

    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Genders : byte
    {
        Male = 1,
        Female = 2,
        Any = 3,
        Neutral = 4,
    }
}
