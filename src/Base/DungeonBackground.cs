using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace UAlbion.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DungeonBackground : ushort
    {
        EarlyGameS = 1,
        EarlyGameL = 2,
        LateGame = 3,
    }
}
