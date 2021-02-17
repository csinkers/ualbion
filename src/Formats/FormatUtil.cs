using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats
{
    public static class FormatUtil
    {
        public static readonly Encoding AlbionEncoding = Encoding.GetEncoding(850);
        public static string BytesTo850String(byte[] bytes) =>
            AlbionEncoding
                .GetString(bytes)
                .Replace("×", "ß")
                .TrimEnd((char) 0);

        public static byte[] BytesFrom850String(string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            return AlbionEncoding.GetBytes(str.Replace("ß", "×"));
        }

        public static string WordWrap(string s, int maxLine)
        {
            if (s == null || s.Length <= maxLine)
                return s;

            int n = 0;
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                n = c == '\n' ? 0 : n + 1;

                sb.Append(c);
                if (n == maxLine)
                {
                    sb.AppendLine();
                    n = 0;
                }
            }
            return sb.ToString();
        }

        public static (byte, byte, byte, byte) UnpackColor(uint c)
        {
            var r = (byte)(c & 0xff);
            var g = (byte)((c >> 8) & 0xff);
            var b = (byte)((c >> 16) & 0xff);
            var a = (byte)((c >> 24) & 0xff);
            return (r, g, b, a);
        }

        public static uint PackColor(byte r, byte g, byte b, byte a) =>
            r
            | (uint)(g << 8)
            | (uint)(b << 16)
            | (uint)(a << 24);

        public static (ushort, ushort) FromPacked(byte b1, byte b2, byte b3)
        {
            ushort overlay = (ushort)((b1 << 4) + (b2 >> 4));
            ushort underlay = (ushort)(((b2 & 0x0F) << 8) + b3);
            return (underlay, overlay);
        }

        public static (byte, byte, byte) ToPacked(ushort underlay, ushort overlay)
        {
            byte b1 = (byte)(overlay >> 4);
            byte b2 = (byte)(((overlay & 0xf) << 4) | ((underlay & 0xf00) >> 8));
            byte b3 = (byte)(underlay & 0xff);
            return (b1, b2, b3);
        }

        public static byte[] ToPacked(int width, int height, int[] underlay, int[] overlay)
        {
            if (width == 0 || height == 0)
                return null;
            if (underlay == null) throw new ArgumentNullException(nameof(underlay));
            if (overlay == null) throw new ArgumentNullException(nameof(overlay));

            var buf = new byte[3 * width * height];
            for (int i = 0; i < width * height; i++)
                (
                    buf[i * 3],
                    buf[i * 3 + 1],
                    buf[i * 3 + 2]
                ) = ToPacked((ushort)underlay[i], (ushort)overlay[i]);
            return buf;
        }

        public static (int[], int[]) FromPacked(int width, int height, byte[] buf)
        {
            if (buf == null) return (null, null);
            if (buf.Length != 3 * width * height)
            {
                throw new InvalidOperationException(
                    "Tried to set raw map data with incorrect " +
                    $"size (expected {3 * width * height} bytes for a {width}x{height} " +
                    $"map but was given {buf.Length})");
            }

            var underlay = new int[width * height];
            var overlay = new int[width * height];
            for (int i = 0; i < width * height; i++)
            {
                (underlay[i], overlay[i]) = FromPacked(
                    buf[i * 3],
                    buf[i * 3 + 1],
                    buf[i * 3 + 2]);
            }

            return (underlay, overlay);
        }

        public static int ParseHex(string s) =>
            s != null && s.StartsWith("0x", StringComparison.InvariantCulture)
                ? int.Parse(s.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)
                : s == null ? 0 : int.Parse(s, CultureInfo.InvariantCulture);

        public static List<(int, int)> SortedIntsToRanges(IEnumerable<int> values) // pairs = (subItemId, count)
        {
            var ranges = new List<(int, int)>();
            if (values == null)
                return ranges;

            int last = int.MinValue;
            int start = int.MinValue;
            foreach (var value in values)
            {
                if (value < last)
                    throw new ArgumentOutOfRangeException(nameof(values), $"A non-sorted list was passed to {nameof(SortedIntsToRanges)}");

                if (value == last) // Ignore duplicates
                    continue;

                if (start == int.MinValue)
                {
                    start = value;
                }
                else if (last != value - 1)
                {
                    ranges.Add((start, last - start + 1));
                    start = value;
                }

                last = value;
            }

            if (start != int.MinValue)
                ranges.Add((start, last - start + 1));

            return ranges;
        }

        public static StringId ResolveTextId(TextId id)
        {
            var result = AssetMapping.Global.TextIdToStringId(id);
            return result.HasValue 
                ? new StringId(result.Value.Item1, result.Value.Item2) 
                : new StringId(id, 0);
        }
    }
}
