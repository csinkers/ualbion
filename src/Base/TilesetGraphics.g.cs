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
    public enum TilesetGraphics : ushort
    {
        Outdoors = 1,
        Outdoors2 = 2,
        IskaiIndoors = 3,
        Desert = 4,
        Stoney = 5,
        Stoney2 = 6,
        CeltDungeon = 7,
        Toronto = 8,
        Celtic = 9,
        DjiKantos = 10,
        Unknown11 = 11,
    }
}
#pragma warning restore CA1707 // Identifiers should not contain underscores
