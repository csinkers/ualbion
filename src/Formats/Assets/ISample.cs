using System;

namespace UAlbion.Formats.Assets
{
    public interface ISample
    {
        int SampleRate { get; }
        int Channels { get; }
        int BytesPerSample { get; }
        ReadOnlySpan<byte> Samples { get; }
    }
}
