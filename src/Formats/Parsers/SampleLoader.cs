using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class SampleLoader : IAssetLoader<AlbionSample>
    {
        const int SampleRate = 11025;
        const int Channels = 1;
        const int BytesPerSample = 1;

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
            return new()
            {
                SampleRate = SampleRate,
                Channels= Channels,
                BytesPerSample = BytesPerSample,
                Samples = s.Bytes(nameof(AlbionSample.Samples), null, (int)s.BytesRemaining)
            };
        }

        static AlbionSample Write(AlbionSample sample, ISerializer s)
        {
            if (sample == null) throw new ArgumentNullException(nameof(sample));
            if(sample.SampleRate != SampleRate)
                throw new ArgumentOutOfRangeException(nameof(sample), $"Only sounds with a sample rate of {SampleRate} are currently supported");
            if(sample.Channels!= Channels)
                throw new ArgumentOutOfRangeException(nameof(sample), "Only sounds with a single channel are currently supported");
            if(sample.BytesPerSample != BytesPerSample)
                throw new ArgumentOutOfRangeException(nameof(sample), $"Only sounds with {BytesPerSample} bytes per sample are currently supported");

            // TODO: Conversion down to 11kHz/1/1 when saving higher quality sound effects to be used by the original game

            s.Bytes(nameof(sample.Samples), sample.Samples, sample.Samples.Length);
            return sample;
        }
    }
}
