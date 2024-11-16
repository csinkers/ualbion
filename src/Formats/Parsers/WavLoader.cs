using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class WavLoader : IAssetLoader<ISample>
{
    public ISample Serdes(ISample existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (s.IsWriting() && existing == null)
            throw new ArgumentNullException(nameof(existing));

        existing ??= new AlbionSample();

        var tag = s.FixedLengthString("Tag", "RIFF", 4); // Container format chunk
        ApiUtil.Assert(tag == "RIFF", "tag == 'RIFF'");

        var riffSizeOffset = s.Offset;
        int _ = s.Int32("TotalSize", 0); // Dummy write to start with, will be overwritten at the end.

        tag = s.FixedLengthString(null, "WAVE",4);
        ApiUtil.Assert(tag == "WAVE", "tag == 'WAVE'");

        SerdesFormatTag(existing, s);
        SerdesDataTag(existing, s);

        if (s.IsWriting())
        {
            int offset = (int)s.Offset;
            s.Seek(riffSizeOffset);
            s.UInt32("TotalSize", (uint)s.Offset - 4); // Write actual length to container format chunk
            s.Seek(offset);
        }
        // else ApiUtil.Assert(fullSize == (int)s.Offset - 4, "Full size of WAV doesn't match bytes read"); // Lots of these in the original game's wav libraries

        return existing;
    }

    static void SerdesFormatTag(ISample w, ISerdes s)
    {
        var tag = s.FixedLengthString(null, "fmt ",4); // Subchunk1 (format metadata)
        ApiUtil.Assert(tag == "fmt ", "tag == 'fmt '");

        s.Int32(null, 16);

        int format = s.UInt16("Format", 1); // Format = Linear Quantisation
        ApiUtil.Assert(format == 1, "format == 1");

        w.Channels = s.UInt16(nameof(w.Channels), (ushort)w.Channels); // NumChannels
        w.SampleRate = s.Int32(nameof(w.SampleRate), w.SampleRate ); // SampleRate
        s.Int32("ByteRate", w.SampleRate * w.Channels * w.BytesPerSample); // ByteRate
        s.Int16("BlockAlign",  (short)(w.Channels * w.BytesPerSample)); // BlockAlign
        w.BytesPerSample = s.UInt16("BitsPerSample", (ushort)(w.BytesPerSample * 8)) / 8; // BitsPerSample
    }

    static void SerdesDataTag(ISample w, ISerdes s)
    {
        var tag = s.FixedLengthString("Tag", "data", 4); // Subchunk2 (raw sample data)
        ApiUtil.Assert(tag == "data");
        int sampleCount = s.Int32("SampleCount", w.Samples?.Length ?? 0);
        w.Samples = s.Bytes(nameof(w.Samples), w.Samples, sampleCount);
    }

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((ISample) existing, s, context);
}
