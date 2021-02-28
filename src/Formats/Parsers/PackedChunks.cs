using System;
using System.Collections.Generic;
using SerdesNet;

namespace UAlbion.Formats.Parsers
{
    public static class PackedChunks
    {
        const string Magic = "CHUNKED_MAGIC";

        // Format: u32 count; [ u32 size; byte[size] chunk ]
        public static IEnumerable<byte[]> Unpack(ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var initial = s.Offset;
            if (s.BytesRemaining < Magic.Length)
            {
                yield return s.ByteArray(null, null, (int)s.BytesRemaining);
                yield break;
            }

            var magic = s.FixedLengthString(null, null, Magic.Length);
            if (!string.Equals(magic, Magic, StringComparison.Ordinal))
            {
                s.Seek(initial);
                yield return s.ByteArray(null, null, (int)s.BytesRemaining);
                yield break;
            }

            var count = s.Int32(null, 0);
            for(int i = 0; i < count; i++)
            {
                int length = s.Int32(null, 0);
                var chunk = s.ByteArray(null, null, length);
                yield return chunk;
            }
        }

        public static void Pack(ISerializer s, int count, Func<int, byte[]> buildChunk)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            s.FixedLengthString(null, Magic, Magic.Length);
            s.Int32(null, count);
            for(int i = 0; i < count; i++)
            {
                var chunk = buildChunk(i);
                s.Int32(null, chunk.Length);
                s.ByteArray(null, chunk, chunk.Length);
            }
        }
    }
}