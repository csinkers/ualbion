using System;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class SongLoader : IAssetLoader<byte[]>
{
    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes((byte[])existing, info, s, context);

    public byte[] Serdes(byte[] existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (s.IsReading())
            return s.Bytes(null, null, (int) s.BytesRemaining);

        if (existing == null) throw new ArgumentNullException(nameof(existing));
        s.Bytes(null, existing, existing.Length);
        return existing;
    }
}
