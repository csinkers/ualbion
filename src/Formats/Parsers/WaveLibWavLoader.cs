using System;
using System.Globalization;
using System.Text.RegularExpressions;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using static System.FormattableString;

namespace UAlbion.Formats.Parsers;

public class WaveLibWavLoader : IAssetLoader<WaveLib>
{
    static readonly WavLoader WavLoader = new();
    static readonly Regex NameRegex = new(@"i(\d+)t(\d+)");
    public WaveLib Serdes(WaveLib existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        return s.IsWriting() 
            ? Write(existing, info, s, context) 
            : Read(s, context);
    }

    static WaveLib Read(ISerializer s, LoaderContext context)
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

            var instrument = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            var type = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
            var sample = FormatUtil.DeserializeFromBytes(bytes, s2 => WavLoader.Serdes(null, null, s2, context));
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

    static WaveLib Write(WaveLib existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (existing == null) throw new ArgumentNullException(nameof(existing));

        PackedChunks.PackNamed(s, WaveLib.MaxSamples, i =>
        {
            var sample = existing.Samples[i];
            if (!sample.Active)
                return (Array.Empty<byte>(), null);

            string extension = Invariant($"i{sample.Instrument}t{sample.Type}");
            var pattern = info.GetPattern(AssetProperty.Pattern, "{0}_{1}_{2}.dat");
            var name = pattern.Format(new AssetPath(info, i, extension));
            var bytes = FormatUtil.SerializeToBytes(s2 => WavLoader.Serdes(sample, null, s2, context));
            return (bytes, name);
        });
        return existing;
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes((WaveLib)existing, info, s, context);
}
