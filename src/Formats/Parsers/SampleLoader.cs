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
            return s.IsWriting() ? Write(existing, s) : Read(s);
        }

        static AlbionSample Read(ISerializer s)
        {
            return new AlbionSample
            {
                Samples = s.Bytes(nameof(AlbionSample.Samples), null, (int)s.BytesRemaining)
            };
        }

        static AlbionSample Write(AlbionSample sample, ISerializer s)
        {
            if (sample == null) throw new ArgumentNullException(nameof(sample));
            s.Bytes(nameof(sample.Samples), sample.Samples, sample.Samples.Length);
            return sample;
        }

    }
}
