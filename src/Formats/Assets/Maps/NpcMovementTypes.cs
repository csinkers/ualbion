using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets.Maps
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NpcMovementTypes : byte
    {
        RandomMask = 3,
        Random1 = 1,
        Random2 = 2,
        Unk4 = 4,
        Stationary = 8,
    }
}
