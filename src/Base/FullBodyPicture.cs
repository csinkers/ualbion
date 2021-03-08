using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace UAlbion.Base
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FullBodyPicture : byte
    {
        Tom = 1,
        Rainer = 2,
        Drirr = 3,
        Sira = 4,
        Mellthas = 5,
        Harriet = 6,
        Joe = 7,
        Unknown8 = 8,
        Khunag = 9,
        Siobhan = 10
    }
}
