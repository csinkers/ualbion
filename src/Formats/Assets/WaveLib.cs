using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;

namespace UAlbion.Formats.Assets
{
    public class WaveLib
    {
        Dictionary<int, WaveLibSample> _instrumentIndex;
        WaveLib() {}
        public static WaveLib Serdes(WaveLib w, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            w ??= new WaveLib();
            w.Samples ??= new WaveLibSample[512];
            s.List(nameof(w.Samples), w.Samples, 512, WaveLibSample.Serdes);

            foreach (var header in w.Samples.Where(x => x.IsValid != -1))
                header.Samples = s.ByteArray(nameof(header.Samples), header.Samples.ToArray(), (int)header.Length);

            return w;
        }

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

        public WaveLibSample[] Samples { get; private set; }
    }
}
