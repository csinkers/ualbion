using System;

namespace UAlbion.Formats.Assets
{
    public class AlbionSample : ISample
    {
        readonly byte[] _samples;
        public AlbionSample(byte[] samples) => _samples = samples;
        public int SampleRate => 11025;
        public int Channels => 1;
        public int BytesPerSample => 1;
        public ReadOnlySpan<byte> Samples => _samples;
    }
}
