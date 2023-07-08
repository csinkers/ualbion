using System;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class RawLoader : IAssetLoader<byte[]>
{
    public byte[] Serdes(byte[] existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        return s.Bytes(null, existing, (int) (existing?.Length ?? s.BytesRemaining));
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((byte[]) existing, s, context);
}
