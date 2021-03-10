using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class SampleLoader : IAssetLoader<AlbionSample>
    {
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionSample) existing, info, mapping, s);

        public AlbionSample Serdes(AlbionSample existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (info == null) throw new ArgumentNullException(nameof(info));
            return new AlbionSample
            {
                Samples = s.ByteArray("Samples", existing?.Samples, (int)s.BytesRemaining)
            };
        }
    }
}
