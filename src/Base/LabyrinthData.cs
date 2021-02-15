using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace UAlbion.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LabyrinthData : byte
    {
        Test1 = 1,
        Test2 = 2,
        Test3 = 3,
        Test4 = 4,
        Test5 = 5,
        TestArgim = 6,
        Unknown7 = 7,
        Unknown8 = 8,
        Unknown9 = 9,

        Unknown100 = 100,
        TestSrimalinar = 101,
        Toronto1 = 102,
        Toronto2 = 103,
        TestDrinno = 104,
        Unknown105 = 105,
        Argim = 106,
        Unknown107 = 107,
        TestKenget = 108,
        Jirinaar = 109,
        Test110 = 110,
        HunterCellar = 111,
        Drinno = 112,
        Unknown113 = 113,
        ArgimDead = 114,
        Unknown115 = 115,
        Unknown116 = 116,
        TransportCaves = 117,
        Kenget1 = 118,
        Kenget2 = 119,
        Kenget3 = 120,
        Kenget4 = 121,
        Kenget5 = 122,
        Kenget6 = 123,
        Kenget7 = 124,
        Kenget8 = 125,

        KounosCave = 200,
        Unknown201 = 201,
        Kontos = 202,
        Beloveno = 203,
        Srimalinar = 204,
        Unknown205 = 205,
        UmajoKenta = 206,
        UmajoPrison = 207,
        DeviceMaker = 208,
        Unknown209 = 209,
        MountainPass = 210,
    }
}
