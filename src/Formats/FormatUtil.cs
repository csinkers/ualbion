using System;
using System.Text;

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
                ) = ToPacked((ushort)(underlay[i] + 1), (ushort)(overlay[i] + 1));
            return buf;
        }

        public static (int[], int[]) FromPacked(int width, int height, byte[] buf)
        {
            if (buf == null) throw new ArgumentNullException(nameof(buf));
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
    }
}
