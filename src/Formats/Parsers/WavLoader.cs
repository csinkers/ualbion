using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class WavLoader : IAssetLoader<AlbionSample>
    {
        public AlbionSample Serdes(AlbionSample w, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (s.IsWriting() && w == null)
                throw new ArgumentNullException(nameof(w));

            w ??= new AlbionSample();

            var tag = s.FixedLengthString("Tag", "RIFF", 4); // Container format chunk
            ApiUtil.Assert(tag == "RIFF", "tag == 'RIFF'");

            var riffSizeOffset = s.Offset;
            int fullSize = s.Int32("TotalSize", 0); // Dummy write to start with, will be overwritten at the end.

            tag = s.FixedLengthString(null, "WAVE",4);
            ApiUtil.Assert(tag == "WAVE", "tag == 'WAVE'");

            SerdesFormatTag(w, s);
            SerdesDataTag(w, s);

            if (s.IsWriting())
            {
                int offset = (int)s.Offset;
                s.Seek(riffSizeOffset);
                s.UInt32("TotalSize", (uint)s.Offset - 4); // Write actual length to container format chunk
                s.Seek(offset);
            }
            else ApiUtil.Assert(fullSize == (int)s.Offset - 4, "Full size of WAV doesn't match bytes read");

            return w;
        }

        static void SerdesFormatTag(AlbionSample w, ISerializer s)
        {
            var tag = s.FixedLengthString(null, "fmt ",4); // Subchunk1 (format metadata)
            ApiUtil.Assert(tag == "fmt ", "tag == 'fmt '");

            s.Int32(null, 16);

            int format = s.UInt16("Format", 1); // Format = Linear Quantisation
            ApiUtil.Assert(format == 1, "format == 1");

            int channels = s.UInt16(nameof(w.Channels), (ushort)w.Channels); // NumChannels
            ApiUtil.Assert(channels == 1, "channels == 1");

            int sampleRate = s.Int32(nameof(w.SampleRate), w.SampleRate ); // SampleRate
            ApiUtil.Assert(sampleRate == 11025, "sampleRate == 11025");

            int byteRate = s.Int32("ByteRate", w.SampleRate * w.Channels * w.BytesPerSample); // ByteRate
            int blockAlign = s.Int16("BlockAlign",  (short)(w.Channels * w.BytesPerSample)); // BlockAlign

            int bps = s.UInt16("BitsPerSample", (ushort)(w.BytesPerSample * 8)); // BitsPerSample
            ApiUtil.Assert(bps == 8, "bps == 8");
        }

        static void SerdesDataTag(AlbionSample w, ISerializer s)
        {
            var tag = s.FixedLengthString("Tag", "data", 4); // Subchunk2 (raw sample data)
            ApiUtil.Assert(tag == "data");
            int sampleCount = s.Int32("SampleCount", w.Samples?.Length ?? 0);
            w.Samples = s.Bytes(nameof(w.Samples), w.Samples, sampleCount);
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionSample) existing, info, mapping, s);
    }
}
