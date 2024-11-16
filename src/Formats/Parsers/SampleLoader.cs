using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class SampleLoader : IAssetLoader<AlbionSample>
{
    const int SampleRate = 11025;
    const int Channels = 1;
    const int BytesPerSample = 1;

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((AlbionSample) existing, s, context);

    public AlbionSample Serdes(AlbionSample existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        return s.IsWriting() ? Write(existing, s) : Read(s);
    }

    static AlbionSample Read(ISerdes s)
    {
        return new()
        {
            SampleRate = SampleRate,
            Channels= Channels,
            BytesPerSample = BytesPerSample,
            Samples = s.Bytes(nameof(AlbionSample.Samples), null, (int)s.BytesRemaining)
        };
    }

    static AlbionSample Write(AlbionSample sample, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(sample);
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
