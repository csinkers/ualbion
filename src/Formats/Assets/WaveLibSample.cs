using System;
using System.Linq;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.Assets
{
    public class WaveLibSample : ISample
    {
        public int IsValid { get; private set; }
        public int Instrument { get; private set; }
        public int Type2 { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; private set; }
        public int Unk14 { get; set; }
        public int Unk18 { get; set; }
        public int SampleRate { get; private set; } // -1 = Use default sample rate (11025)
        public int Channels => 1;
        public int BytesPerSample => 1;
        public byte[] Samples { get; set; } = Array.Empty<byte>();

        public static WaveLibSample Serdes(int i, WaveLibSample w, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            w ??= new WaveLibSample();
            w.IsValid = s.Int32(nameof(IsValid), w.IsValid);
            w.Instrument = s.Int32(nameof(Instrument), w.Instrument);
            w.Type2 = s.Int32(nameof(Type2), w.Type2);
            w.Offset = s.UInt32(nameof(Offset), w.Offset);
            w.Length = s.UInt32(nameof(Length), w.Length);
            w.Unk14 = s.Int32(nameof(Unk14), w.Unk14);
            w.Unk18 = s.Int32(nameof(Unk18), w.Unk18);
            w.SampleRate = s.Int32(nameof(SampleRate), w.SampleRate);

            // Check for new patterns
            ApiUtil.Assert(w.IsValid == 0 || w.IsValid == -1);
            ApiUtil.Assert(new[] { 119, 120, 121, 122, 123, 124, 125, 126, 127, -1 }.Contains(w.Instrument));
            ApiUtil.Assert(new[] { 56, 58, 60, 62, 63, 64, 66, 69, 76, 80 }.Contains(w.Type2));
            ApiUtil.Assert(w.Unk14 == 0);
            ApiUtil.Assert(w.Unk18 == 0);
            ApiUtil.Assert(w.SampleRate == 11025 || w.SampleRate == -1);

            return w;
        }
    }
}
