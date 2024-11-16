using System;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class SongLoader : IAssetLoader<byte[]>
{
    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((byte[])existing, s, context);

    public byte[] Serdes(byte[] existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (s.IsReading())
            return s.Bytes(null, null, (int) s.BytesRemaining);

        ArgumentNullException.ThrowIfNull(existing);
        s.Bytes(null, existing, existing.Length);
        return existing;
    }
}
