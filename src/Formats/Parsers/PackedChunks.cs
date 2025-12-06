using System;
using System.Collections.Generic;
using SerdesNet;

namespace UAlbion.Formats.Parsers;

public static class PackedChunks
{
    const string Magic = "CHUNKED_MAGIC";

    // Format: u32 count; [ u32 size; byte[size] chunk ]
    public static IEnumerable<(byte[], string)> Unpack(ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        var initial = s.Offset;
        if (s.BytesRemaining < Magic.Length)
        {
            yield return (s.Bytes(null, null, (int)s.BytesRemaining), null);
            yield break;
        }

        var magic = s.AlbionString(null, null, Magic.Length);
        if (!string.Equals(magic, Magic, StringComparison.Ordinal))
        {
            s.Seek(initial);
            yield return (s.Bytes(null, null, (int)s.BytesRemaining), null);
            yield break;
        }

        var count = s.Int32(null, 0);
        for (int i = 0; i < count; i++)
        {
            int nameLength = s.Int32(i, 0);
            var name = s.AlbionString(i, string.Empty, nameLength);
            int length = s.Int32(null, 0);
            var chunk = s.Bytes(null, null, length);
            yield return (chunk, name);
        }
    }

    public static void Pack(ISerdes s, int count, Func<int, byte[]> buildChunk)
        => PackNamed(s, count, i => (buildChunk(i), null));

    public static void PackNamed(ISerdes s, int count, Func<int, (ReadOnlyMemory<byte>, string)> buildChunk)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(buildChunk);

        if (count == 1)
        {
            var (chunk, _) = buildChunk(0);
            s.Bytes(null, chunk.ToArray());
            return;
        }

        s.AlbionString(null, Magic, Magic.Length);
        s.Int32(null, count);

        for(int i = 0; i < count; i++)
        {
            var (chunk, name) = buildChunk(i);
            int nameLength = s.Int32(i, name?.Length ?? 0);
            s.AlbionString(i, name ?? string.Empty, nameLength);
            s.Int32(null, chunk.Length);
            s.Bytes(null, chunk.ToArray());
        }
    }
}
