using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats
{
    public static class FormatUtil
    {
        public static readonly Encoding AlbionEncoding;

        [SuppressMessage("Performance",
            "CA1810:Initialize reference type static fields inline",
            Justification = "Encoding.GetEncoding must happen after Encoding.RegisterProvider")]
        static FormatUtil()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider
                .Instance); // Required for code page 850 support in .NET Core
            PerfTracker.StartupEvent("Registered encodings");
            AlbionEncoding = Encoding.GetEncoding(850);
        }

        public static string BytesTo850String(byte[] bytes) =>
            AlbionEncoding
                .GetString(bytes)
                .Replace("×", "ß")
                .TrimEnd((char)0);

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

        public static byte[] ToPacked(int[] underlay, int[] overlay, int adjust = 0)
        {
            if (underlay == null) throw new ArgumentNullException(nameof(underlay));
            if (overlay == null) throw new ArgumentNullException(nameof(overlay));

            if (underlay.Length != overlay.Length)
            {
                throw new ArgumentOutOfRangeException(
                    "Tried to pack tiledata, but the underlay count " +
                    $"({underlay.Length}) differed from the overlay count ({overlay.Length})");
            }

            var buf = new byte[3 * underlay.Length];
            for (int i = 0; i < underlay.Length; i++)
                (
                    buf[i * 3],
                    buf[i * 3 + 1],
                    buf[i * 3 + 2]
                ) = ToPacked((ushort)(underlay[i] + adjust), (ushort)(overlay[i] + adjust));
            return buf;
        }

        public static (int[], int[]) FromPacked(byte[] buf, int adjust = 0)
        {
            if (buf == null) return (null, null);
            if (buf.Length % 3 != 0)
            {
                throw new InvalidOperationException(
                    "Tried to set raw map data with incorrect " +
                    "size (expected a multiple of 3, " +
                    $"but was given {buf.Length})");
            }

            int tileCount = buf.Length / 3;
            var underlay = new int[tileCount];
            var overlay = new int[tileCount];
            for (int i = 0; i < tileCount; i++)
            {
                (underlay[i], overlay[i]) = FromPacked(
                    buf[i * 3],
                    buf[i * 3 + 1],
                    buf[i * 3 + 2]);

                underlay[i] += adjust;
                overlay[i] += adjust;
            }

            return (underlay, overlay);
        }

        public static int ParseHex(string s) =>
            s != null && s.StartsWith("0x", StringComparison.InvariantCulture)
                ? int.Parse(s.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)
                : s == null
                    ? 0
                    : int.Parse(s, CultureInfo.InvariantCulture);


        const string HexChars = "0123456789ABCDEF";

        public static string BytesToHexString(ReadOnlySpan<byte> bytes)
        {
            if (bytes == null) return "";
            var result = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                result.Append(HexChars[b >> 4]);
                result.Append(HexChars[b & 0xf]);
            }

            return result.ToString();
        }

        static byte HexCharToByte(char c)
        {
            if (c >= 0 && c <= 9) return (byte)(c - '0');
            if (c >= 'A' && c <= 'F') return (byte)(c - 'A');
            throw new FormatException($"Invalid character '{c}' in hex string");
        }

        public static byte[] HexStringToBytes(string s)
        {
            if (string.IsNullOrEmpty(s)) return Array.Empty<byte>();
            if ((s.Length & 1) == 1)
                throw new FormatException("Hex string did not consist of an even number of characters");

            var result = new byte[s.Length / 2];
            for (int i = 0, j = 0; i < result.Length; i++)
            {
                char c1 = char.ToUpperInvariant(s[j++]);
                char c2 = char.ToUpperInvariant(s[j++]);
                result[i] = (byte)((HexCharToByte(c1) << 4) | HexCharToByte(c2));
            }

            return result;
        }

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
                    throw new ArgumentOutOfRangeException(nameof(values),
                        $"A non-sorted list was passed to {nameof(SortedIntsToRanges)}");

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

        static readonly char[] NewLineChars = { '\n', '\r' };

        public static string[] SplitLines(string s)
            => s?.Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries)
               ?? Array.Empty<string>();

        public static bool Compare(QueryOperation operation, int value, int immediate) =>
            operation switch
            {
                QueryOperation.IsTrue => value != 0,
                QueryOperation.NotEqual => value != immediate,
                QueryOperation.OpUnk2 => value == immediate,
                QueryOperation.Equals => value == immediate,
                QueryOperation.GreaterThanOrEqual => value >= immediate,
                QueryOperation.GreaterThan => value > immediate,
                QueryOperation.OpUnk6 => value == immediate,
                _ => true
            };

        public static void Blit(ReadOnlySpan<byte> from, Span<byte> to, int width, int height, int fromStride,
            int toStride)
        {
            int srcIndex = 0;
            int destIndex = 0;
            for (int i = 0; i < height; i++)
            {
                var row = from.Slice(srcIndex, width);
                row.CopyTo(to.Slice(destIndex));
                srcIndex += fromStride;
                destIndex += toStride;
            }
        }

        public static byte[] BytesFromTextWriter(Action<TextWriter> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            using var ms = new MemoryStream();
            using var tw = new StreamWriter(ms);
            func(tw);
            tw.Flush();
            ms.Position = 0;
            return ms.ToArray();
        }

        public static byte[] BytesFromStream(Action<Stream> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            using var stream = new MemoryStream();
            func(stream);
            stream.Position = 0;
            return stream.ToArray();
        }

        public static ISerializer SerializeWithSerdes(Action<ISerializer> serdes)
        {
            if (serdes == null) throw new ArgumentNullException(nameof(serdes));
            var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, AlbionEncoding, true);
            using var s = new AlbionWriter(bw);
            serdes(s);
            bw.Flush();
            ms.Position = 0;

            var br = new BinaryReader(ms);
            return new AlbionReader(br, ms.Length, () =>
            {
                br.Dispose();
                ms.Dispose();
            });
        }

        public static byte[] SerializeToBytes(Action<ISerializer> serdes)
        {
            if (serdes == null) throw new ArgumentNullException(nameof(serdes));
            var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, AlbionEncoding, true);
            using var s = new AlbionWriter(bw);
            serdes(s);
            bw.Flush();
            ms.Position = 0;
            return ms.ToArray();
        }

        public static T DeserializeFromBytes<T>(byte[] bytes, Func<ISerializer, T> serdes)
        {
            if (serdes == null) throw new ArgumentNullException(nameof(serdes));
            using var ms = new MemoryStream(bytes);
            var br = new BinaryReader(ms);
            using var s = new AlbionReader(br);
            return serdes(s);
        }
    }
}

