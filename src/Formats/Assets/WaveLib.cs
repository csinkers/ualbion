using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;

namespace UAlbion.Formats.Assets
{
    public class WaveLib
    {
        public const int MaxSamples = 512;
        Dictionary<int, WaveLibSample> _instrumentIndex;
        public static WaveLib Serdes(WaveLib w, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            w ??= new WaveLib();
            w.Samples ??= new WaveLibSample[512];
            uint offset = WaveLibSample.SizeInBytes * MaxSamples;
            s.List(nameof(w.Samples), w.Samples, 512, (_, x, s2) => WaveLibSample.Serdes(x, s2, ref offset));

            foreach (var header in w.Samples.Where(x => x.Active))
                header.Samples = s.Bytes(nameof(header.Samples), header.Samples, header.Samples.Length);

            return w;
        }

        public WaveLibSample[] Samples { get; private set; } = new WaveLibSample[MaxSamples];
        public void ClearInstrumentIndex() => _instrumentIndex = null; // Should be called if the instrument mapping is changed

        public ISample this[int instrument]
        {
            get
            {
                _instrumentIndex ??= Samples
                    .ToLookup(x => x.Instrument)
                    .ToDictionary(x => x.Key, x => x.First());

                return _instrumentIndex.TryGetValue(instrument, out var sample) ? sample : null;
            }
        }

    }
}
