using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class SampleLoader : IAssetLoader<AlbionSample>
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionSample) existing, config, mapping, s);

        public AlbionSample Serdes(AlbionSample existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));
            return new AlbionSample(s.ByteArray(null, null, (int)s.BytesRemaining));
        }
    }
}
