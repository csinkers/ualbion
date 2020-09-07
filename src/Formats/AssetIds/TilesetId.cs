// Note: This file was automatically generated using Tools/GenerateEnums.
// No changes should be made to this file by hand. Instead, the relevant json
// files should be modified and then GenerateEnums should be used to regenerate
// the various ID enums.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace UAlbion.Formats.AssetIds
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TilesetId : byte
    {
        Outdoors = 0,
        Outdoors2 = 1,
        Indoors = 2,
        Desert = 3,
        Stoney = 4,
        Stoney2 = 5,
        CeltDungeon = 6,
        Toronto = 7,
        Celtic = 8,
        DjiKantos = 9,
        Unknown10 = 10,
    }
}
#pragma warning restore CA1707 // Identifiers should not contain underscores
