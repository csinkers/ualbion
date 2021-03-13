using System;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    public class RawLoader : IAssetLoader<byte[]>
    {
        public byte[] Serdes(byte[] existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            return s.Bytes(null, existing, (int) (existing?.Length ?? s.BytesRemaining));
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((byte[]) existing, info, mapping, s);
    }
}
