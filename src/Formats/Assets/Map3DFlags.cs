using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Map3DFlags : byte
    {
        Unk0 = 1,
        Unk1 = 1 << 1,
        Unk2 = 1 << 2,
        Unk3 = 1 << 3,
        Unk4 = 1 << 4,
        Unk5 = 1 << 5,
        Unk6 = 1 << 6,
        Unk7 = 1 << 7,
    }
}