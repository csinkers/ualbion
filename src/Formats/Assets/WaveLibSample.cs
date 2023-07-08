using System;
using System.Linq;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.Assets;

public class WaveLibSample : ISample
{
    public const uint SizeInBytes = 32;
    public bool Active { get; set; }
    public int Instrument { get; set; } = -1;
    public int Type { get; set; } = 60;

    public int SampleRate { get; set; } = -1; // -1 = Use default sample rate (11025)
    public int Channels { get; set; } = 1;
    public int BytesPerSample { get; set; } = 1;
    public byte[] Samples { get; set; } = Array.Empty<byte>();

    public override string ToString() => Active ? $"I:{Instrument} T:{Type} {Samples.Length} = {(MidiInstrument)Instrument}" : "None";

    public static WaveLibSample Serdes(WaveLibSample w, ISerializer s, ref uint nextBufferOffset)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        w ??= new WaveLibSample();
        w.Active = s.Int32(nameof(Active), w.Active ? 0 : -1) == 0; // 0
        w.Instrument = s.Int32(nameof(Instrument), w.Instrument); // 4
        w.Type = s.Int32(nameof(Type), w.Type); // 8

        uint offset = s.UInt32("Offset", w.Active ? nextBufferOffset : 0); // c
        int length = s.Int32("Length", w.Samples.Length); // 10
        if (w.Samples.Length == 0 && length > 0)
            w.Samples = new byte[length];

        if (length > 0)
            ApiUtil.Assert(offset == nextBufferOffset);
        nextBufferOffset += (uint)length;

        int zeroed = s.Int32(null, 0); // 14
        zeroed += s.Int32(null, 0); // 18
        if (zeroed != 0) throw new FormatException("Expected header fields 14/18 to be 0");

        w.SampleRate = s.Int32(nameof(SampleRate), w.SampleRate); // 1c

        // Check for new patterns
        ApiUtil.Assert(new[] { 119, 120, 121, 122, 123, 124, 125, 126, 127, -1 }.Contains(w.Instrument));
        ApiUtil.Assert(new[] { 56, 58, 60, 62, 63, 64, 66, 69, 76, 80 }.Contains(w.Type));
        ApiUtil.Assert(w.SampleRate is 11025 or -1);

        return w;
    }
}
