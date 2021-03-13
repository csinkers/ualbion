using System;

namespace UAlbion.Api.Visual
{
    public delegate ReadOnlyByteImageBuffer GetByteFrameDelegate(int frame);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Never compared")]
    public readonly ref struct ReadOnlyByteImageBuffer
    {
        public ReadOnlyByteImageBuffer(int width, int height, int stride, ReadOnlySpan<byte> buffer)
        {
            Width = width;
            Height = height;
            Stride = stride;
            Buffer = buffer;
        }

        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public ReadOnlySpan<byte> Buffer { get; }
    }
}
