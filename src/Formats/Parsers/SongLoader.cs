using System;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    public class SongLoader : IAssetLoader
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            return s.ByteArray(null, null, (int)s.BytesRemaining);
        }
    }
}
