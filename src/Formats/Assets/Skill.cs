using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Skill : byte
    {
        Melee = 0,
        Ranged = 1,
        CriticalChance = 2,
        LockPicking = 3,
    }
}
