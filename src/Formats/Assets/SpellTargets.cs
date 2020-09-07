using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SpellTargets : byte
    {
        Party         = 1 << 0,
        Unk1          = 1 << 1,
        DeadParty     = 1 << 2,
        OneMonster    = 1 << 3,
        RowOfMonsters = 1 << 4,
        AllMonsters   = 1 << 5,
        Unk6          = 1 << 6,
        MapTile       = 1 << 7,
    }
}
