using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats;

public static class FormatUtil
{
    public static readonly Encoding AlbionEncoding = GetEncoding850();
    static Encoding GetEncoding850()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core
        PerfTracker.StartupEvent("Registered encodings");
        return Encoding.GetEncoding(850);
    }

    public static string BytesTo850String(byte[] bytes) =>
        AlbionEncoding
            .GetString(bytes)
            .Replace('×', 'ß')
            .TrimEnd((char)0);

    public static byte[] BytesFrom850String(string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        return AlbionEncoding.GetBytes(str.Replace('ß', '×'));
    }

    public static bool TryParseFloat(string s, out float result)
        => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

    public static int ParseHex(string s) =>
        s != null && s.StartsWith("0x", StringComparison.InvariantCulture)
            ? int.Parse(s[2..], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)
            : s == null
                ? 0
                : int.Parse(s);


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
        if (c is >= '0' and <= '9') return (byte)(c - '0');
        if (c is >= 'A' and <= 'F') return (byte)(c - 'A' + 10);
        throw new FormatException($"Invalid character '{c}' in hex string");
    }

    public static byte[] HexStringToBytes(string s)
    {
        if (string.IsNullOrEmpty(s)) return [];
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

    public static bool Compare(QueryOperation operation, int value, int immediate) =>
        operation switch
        {
            QueryOperation.NonZero            => value != 0,
            QueryOperation.LessThan           => value < immediate,
            QueryOperation.LessThanOrEqual    => value <= immediate,
            QueryOperation.Equals             => value == immediate,
            QueryOperation.GreaterThanOrEqual => value >= immediate,
            QueryOperation.GreaterThan        => value > immediate,
            _ => true
        };

    public static byte[] BytesFromTextWriter([InstantHandle] Action<TextWriter> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        using var ms = new MemoryStream();
        using var tw = new StreamWriter(ms);
        func(tw);
        tw.Flush();
        ms.Position = 0;
        return ms.ToArray();
    }

    public static byte[] BytesFromStream([InstantHandle] Action<Stream> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        using var stream = new MemoryStream();
        func(stream);
        stream.Position = 0;
        return stream.ToArray();
    }

    public static ISerializer SerializeWithSerdes([InstantHandle] Action<ISerializer> serdes)
    {
        ArgumentNullException.ThrowIfNull(serdes);
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

    public static byte[] SerializeToBytes([InstantHandle] Action<ISerializer> serdes)
    {
        ArgumentNullException.ThrowIfNull(serdes);
        var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms, AlbionEncoding, true);
        using var s = new AlbionWriter(bw);
        serdes(s);
        bw.Flush();
        ms.Position = 0;
        return ms.ToArray();
    }

    public static T DeserializeFromBytes<T>(byte[] bytes, [InstantHandle] Func<ISerializer, T> serdes)
    {
        ArgumentNullException.ThrowIfNull(serdes);
        using var ms = new MemoryStream(bytes);
        var br = new BinaryReader(ms);
        using var s = new AlbionReader(br);
        return serdes(s);
    }

    public static string GetReducedSha256HexString(string filename, IFileSystem disk)
    {
        ArgumentNullException.ThrowIfNull(disk);

        using var sha256 = SHA256.Create();
        using var stream = disk.OpenRead(filename);
        var hashBytes = sha256.ComputeHash(stream);
        return BytesToHexString(hashBytes.AsSpan(0, 4));
    }
}
