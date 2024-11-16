using System;
using System.Text.RegularExpressions;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats.Assets;
using static System.FormattableString;

namespace UAlbion.Formats.Parsers;

public class WaveLibWavLoader : IAssetLoader<WaveLib>
{
    static readonly WavLoader WavLoader = new();
    static readonly Regex NameRegex = new(@"i(\d+)t(\d+)");
    public WaveLib Serdes(WaveLib existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(context);
        return s.IsWriting() 
            ? Write(existing, s, context) 
            : Read(s, context);
    }

    static WaveLib Read(ISerdes s, AssetLoadContext context)
    {
        var lib = new WaveLib();
        int i = 0;
        foreach(var (bytes, name) in PackedChunks.Unpack(s))
        {
            if (bytes == null || bytes.Length == 0)
            {
                lib.Samples[i] = new WaveLibSample();
                i++;
                continue;
            }

            var m = NameRegex.Match(name);
            if (!m.Success)
            {
                throw new FormatException(
                    $"Invalid wavelib entry name \"{name}\". " +
                    "WaveLib entries must be named like A_B_iCtD.wav " +
                    "where A is the wavelib id, B is the sub-id, C is the " +
                    "instrument and D is the type, e.g. 0_5_i129t63.wav");
            }

            var instrument = int.Parse(m.Groups[1].Value);
            var type = int.Parse(m.Groups[2].Value);
            var sample = FormatUtil.DeserializeFromBytes(bytes, s2 => WavLoader.Serdes(null, s2, context));
            lib.Samples[i] = new WaveLibSample
            {
                Active = true,
                Type = type,
                Instrument = instrument,
                SampleRate = sample.SampleRate,
                BytesPerSample = sample.BytesPerSample,
                Channels = sample.Channels,
                Samples = sample.Samples
            };
            i++;
        }

        for (; i < WaveLib.MaxSamples; i++)
            lib.Samples[i] = new WaveLibSample();

        return lib;
    }

    static readonly AssetPathPattern DefaultPattern = AssetPathPattern.Build("{0}_{1}_{2}.dat");
    static WaveLib Write(WaveLib existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(context);

        PackedChunks.PackNamed(s, WaveLib.MaxSamples, i =>
        {
            var sample = existing.Samples[i];
            if (!sample.Active)
                return ([], null);

            string extension = Invariant($"i{sample.Instrument}t{sample.Type}");
            var pattern = context.GetProperty(AssetProps.Pattern, DefaultPattern);
            var name = pattern.Format(context.BuildAssetPath(i, extension));
            var bytes = FormatUtil.SerializeToBytes(s2 => WavLoader.Serdes(sample, s2, context));
            return (bytes, name);
        });
        return existing;
    }

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((WaveLib)existing, s, context);
}
