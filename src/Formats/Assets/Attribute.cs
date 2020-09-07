using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Attribute : byte
    {
        Strength = 0,
        Intelligence = 1,
        Dexterity = 2,
        Speed = 3,
        Stamina = 4,
        Luck = 5,
        MagicResistance = 6,
        MagicTalent = 7,
    }
}
