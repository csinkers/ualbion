using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace UAlbion.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WaveLibrary : byte
    {
        SomewhatMenacing = 1,
        DrumsMarkTreeAndFlute = 2,
        Atmospheric = 3,
        Toronto = 4,
        TorontoAmbient = 5,
        WindyAmbient = 6,
        DjiCantosAmbient = 7,
        JungleAmbient = 8,
        DungeonAmbient = 9,
        TownAmbient = 10,
        Ambient = 11,
        WaterAmbient = 12,
        DocksAmbient = 13,
        ForgeAmbient = 14,
        HummingAmbient = 15,
        PubAmbient = 16,
        PianoAndBassWithFlute = 17,
        Funky = 18,
        CombatMusic = 19,
        TechCombatMusic = 20,
        WindyLivestockAmbient = 21,
        TechAmbient = 22,
        HarpBased = 23,
        JirinaarMusic = 24,
        OutdoorsMusic = 25,
        CombatMusic2 = 26,
        Ethereal = 27,
        SeagullAmbient = 28,
        HummingCrowAmbient = 29,
        EpTension = 30,
        JungleTownAmbient = 31,
        JungleTownAmbient2 = 32,
        JunglePubAmbient = 33,
        DankDungeonAmbient = 34,
        OutdoorsMusic2 = 35,
        WindyLivestockAmbient2 = 36,
        // Unknown37 = 37,
        // Unknown38 = 38,
        // Unknown39 = 39,
        // Unknown40 = 40,
        // Unknown41 = 41,
        // Unknown42 = 42,
        // HalfBrokenMidi = 43,
        // Unknown44 = 44,
        // Unknown45 = 45,
        // ShortOptimisticGlissando = 46,
        // Invalid = 51,
        // Invalid2 = 53,
    }
}
