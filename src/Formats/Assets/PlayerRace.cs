using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlayerRace : byte
    {
        Terran        = 0,
        Iskai         = 1,
        Celt          = 2,
        KengetKamulos = 3,
        DjiCantos     = 4,
        Mahino        = 5,
        Decadent      = 6,
        Umajo         = 7,
        Monster       = 14
    }
}
