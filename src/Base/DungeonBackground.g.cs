// Note: This file was automatically generated using Tools/GenerateEnums.
// No changes should be made to this file by hand. Instead, the relevant json
// files should be modified and then GenerateEnums should be used to regenerate
// the various ID enums.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
#pragma warning disable CA1707 // Identifiers should not contain underscores
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
#pragma warning restore CA1707 // Identifiers should not contain underscores
