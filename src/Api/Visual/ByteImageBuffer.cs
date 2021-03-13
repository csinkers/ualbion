using System;

namespace UAlbion.Api.Visual
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Never compared")]
    public readonly ref struct ByteImageBuffer
    {
        public ByteImageBuffer(int width, int height, int stride, Span<byte> buffer)
        {
            Width = width;
            Height = height;
            Stride = stride;
            Buffer = buffer;
        }

        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public Span<byte> Buffer { get; }
    }
}