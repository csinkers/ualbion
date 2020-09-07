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
    public enum CombatBackgroundId : byte
    {
        Plains = 0,
        Dungeon = 1,
        Desert = 2,
        WoodDungeon = 3,
        BarrelDungeon = 4,
        TableDungeon = 5,
        FireDungeon = 6,
        PlantDungeon = 7,
        Cave = 8,
        Toronto = 9,
        LivingWalls = 10,
        Forest = 11,
        TownNight = 12,
        Town = 13,
        Plains2 = 14,
        Plains3 = 15,
        DesertNight = 16,
        ForestNight = 17,
        ForestNight2 = 18,
    }
}
#pragma warning restore CA1707 // Identifiers should not contain underscores
