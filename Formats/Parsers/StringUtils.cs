using System.Text;

namespace UAlbion.Formats.Parsers
{
    static class StringUtils
    {
        public static string BytesTo850String(byte[] bytes) => Encoding.GetEncoding(850).GetString(bytes).Replace("×", "ß").TrimEnd((char)0);
    }
}
