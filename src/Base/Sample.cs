using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace UAlbion.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Sample : byte
    {
        CreakyWoodenDoor = 1,
        SqueekyMetalDoor = 2,
        HiTechDoor = 3,
        HeavyWoodenDoor = 4,
        Unknown5 = 5,
        Unknown6 = 6,
        Unknown7 = 7,
        Unknown8 = 8,
        Unknown9 = 9,
        WaterWithStatic = 10,
        CracklingFire = 11,
        Stream = 12,
        Unknown13 = 13,
        Unknown14 = 14,
        Unknown15 = 15,
        Unknown16 = 16,
        Unknown17 = 17,
        Unknown18 = 18,
        Unknown19 = 19,
        SlidingStone = 20,
        BucketFilling = 21,
        StoneSwitch = 22,
        ShortSlidingStone = 23,
        Unknown24 = 24,
        HissingSteam = 25,
        ButtonPress = 26,
        MediumSlidingStone = 27,
        ButtonPress2 = 28,
        SlowCreakingDoor = 29,
        EchoeyMetalDoor = 30,
        GratingRaised = 31,
        GratingRaised2 = 32,
        HiTechHiss = 33,
        XylophoneTones = 34,
        MetallicStrike = 35,
        DistantCollapse = 36,
        CrumblingStone = 37,
        Healing = 38,
        Zap = 39,
        Pyew = 40,
        DingALing = 41,
        DoubleHiss = 42,
        LaserPyewPyew = 43,
        AnticipationOfCollapse = 44,
        BreakingSticks = 45,
        VineCoveredRocksFallingAway = 46,
        Switch = 47,
        IllTemperedLlama = 48,
        HiTechHiss2 = 49,
        Unknown50 = 50,
        Unknown51 = 51,
        AmbientThrum = 52,
        AnnoyingNewsTerminal = 53,
        SparkingWires = 54,
        ArcingCircuitBoard = 55,
        Growl = 56,
        Fire = 57,
        BubblingWater = 58,
        Windy = 59,
        Hammering = 60,
        Beep = 61,
        AngryBeep = 62,
        Switch2 = 63,
        ShipAnnouncement = 64,
        WobblyStatic = 65,
        Snoring = 66,
        SighingAndCoughing = 67,
        Waterfall = 68,
        MagicChimes = 69,
        TechBeepBoop = 70,
        SpongeyDoor = 71,
        FallingRock = 72,
        PickaxeChipping = 73,
        Woosh = 74,
        DistantBreakingPottery = 75,
        ThickTwistedRope = 76,
        ZoomBoopityBoopBoop = 77,
        Bweeoop = 78,
        Tick = 100,
        Unknown101 = 101,
        Tock = 102,
        Bzoop = 103,
        Kalunk = 104,
        Quacklequack = 105,
        Shutdown = 106,
        Unknown107 = 107,
        WoogityTack = 108,
        Unknown109 = 109,
        Unknown110 = 110,
        Unknown111 = 111,
        Unknown112 = 112,
        Unknown113 = 113,
        Unknown114 = 114,
        Unknown115 = 115,
        Unknown116 = 116,
        Unknown117 = 117,
        Unknown118 = 118,
        Unknown119 = 119,
        Unknown120 = 120,
        Unknown121 = 121,
        Unknown122 = 122,
        Unknown123 = 123,
        Unknown124 = 124,
        Unknown125 = 125,
        Unknown126 = 126,
        Unknown127 = 127,
        Unknown128 = 128,
        Unknown129 = 129,
        Unknown130 = 130,
        Unknown131 = 131,
        Unknown132 = 132,
        Unknown133 = 133,
        Unknown134 = 134,
        Unknown135 = 135,
        Unknown136 = 136,
        Unknown137 = 137,
        Unknown138 = 138,
        Unknown139 = 139,
        Unknown140 = 140,
        Unknown141 = 141,
        Unknown142 = 142,
        Unknown143 = 143,
        Unknown144 = 144,
        Unknown145 = 145,
        Unknown146 = 146,
        Unknown147 = 147,
        Unknown148 = 148,
        Unknown149 = 149,
        SmallMachine = 150,
        BuzzingInsect = 151,
        Beehive = 152,
        Hissing = 153,
        Sawing = 154,
        Cicadas = 155,
        Crackly = 156,
        Unknown157 = 157,
        Unknown158 = 158,
        SteadyTechDrone = 159,
        HarpGlissandos = 160,
        Pziew = 161,
        Unknown162 = 162,
        Unknown163 = 163,
        Unknown164 = 164,
        Unknown165 = 165,
        Unknown166 = 166,
        Unknown167 = 167,
        Unknown168 = 168,
        Unknown169 = 169,
        Unknown170 = 170,
        Unknown171 = 171,
        Unknown172 = 172,
        Unknown173 = 173,
        Unknown174 = 174,
        Unknown175 = 175,
        Unknown176 = 176,
        Unknown177 = 177,
        Unknown178 = 178,
        Unknown179 = 179,
        Unknown180 = 180,
        Unknown181 = 181,
        Unknown182 = 182,
        Unknown183 = 183,
        Unknown184 = 184,
        Unknown185 = 185,
        Unknown186 = 186,
        Unknown187 = 187,
        Unknown188 = 188,
        Unknown189 = 189,
        Unknown190 = 190,
        Unknown191 = 191,
        Unknown192 = 192,
        Unknown193 = 193,
        Unknown194 = 194,
        Unknown195 = 195,
        Unknown196 = 196,
        Unknown197 = 197,
        Unknown198 = 198,
        Unknown199 = 199,
        Unknown200 = 200,
        ThrowMagicSeed = 201,
        Unknown202 = 202,
        PoweringUp = 203,
        Unknown204 = 204,
        DissonantSting = 205,
        EchoingPing = 206,
        TwoToneFadingIntoDissonance = 207,
        TechTension = 208,
        LaserDoor = 209,
        MiniPyiew = 210,
        Unknown211 = 211,
        Unknown212 = 212,
        BeamMeDown = 213,
        Choonk = 214,
        DiddlyDiddly = 215,
        Drrzh = 216,
        DistantMovement = 217,
        RockCrumbling = 218,
        Takwow = 219,
        Unknown220 = 220,
        Bwoowoo = 221,
        Unknown222 = 222,
        LoHiHiHiHi = 223,
        Unknown224 = 224,
        MultipleImpacts = 225,
        Strings = 226,
        Unknown227 = 227,
        Unknown228 = 228,
        Unknown229 = 229,
        LongGrowlWithLaugh = 230,
        AngryDissonantBuzz = 231,
        CymbalCrescendo = 232,
        Unknown233 = 233,
        Unknown234 = 234,
        Unknown235 = 235,
        Unknown236 = 236,
        Unknown237 = 237,
        Unknown238 = 238,
        Unknown239 = 239,
        Unknown240 = 240,
        Unknown241 = 241,
        Unknown242 = 242,
        Unknown243 = 243,
        Unknown244 = 244,
        Unknown245 = 245,
        Unknown246 = 246,
        Unknown247 = 247,
        Unknown248 = 248,
        Unknown249 = 249,
        Unknown250 = 250,
        Unknown251 = 251,
        Unknown252 = 252,
        Unknown253 = 253,
        Unknown254 = 254,
        Unknown255 = 255,
    }
}