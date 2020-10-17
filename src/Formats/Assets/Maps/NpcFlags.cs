using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets.Maps
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NpcFlags : byte
    {
        NonPartySeeking = 1,
        IsMonster = 1 << 1,
        Unk2 = 1 << 2,
        Unk3 = 1 << 3, // Has contact event?
        Unk4 = 1 << 4,
        Unk5 = 1 << 5,
        Unk6 = 1 << 6,
        Unk7 = 1 << 7,
    }
}
