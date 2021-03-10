using System;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    public class SongLoader : IAssetLoader<byte[]>
    {
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((byte[]) existing, info, mapping, s);

        public byte[] Serdes(byte[] existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            return s.ByteArray(null, existing, (int)s.BytesRemaining);
        }
    }
}
