using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace UAlbion.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Special : ushort
    {
        SoundBank = 0,
        ItemNames = 1,
        SystemStrings = 2,
        UAlbionStrings = 3,
        Words1 = 4,
        Words2 = 5,
        Words3 = 6,
        DummyObject = 7,
    }
}
