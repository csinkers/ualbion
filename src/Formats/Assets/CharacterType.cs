using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CharacterType : byte
    {
        Party = 0,
        Npc = 1,
        Monster = 2
    }
}
