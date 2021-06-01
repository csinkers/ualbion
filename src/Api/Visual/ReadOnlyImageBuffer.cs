using System;

namespace UAlbion.Api.Visual
{
    public delegate ReadOnlyImageBuffer<T> GetFrameDelegate<T>(int frame);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Never compared")]
    public readonly ref struct ReadOnlyImageBuffer<T>
    {
        public ReadOnlyImageBuffer(int width, int height, int stride, ReadOnlySpan<T> buffer)
        {
            Width = width;
            Height = height;
            Stride = stride;
            Buffer = buffer;
        }

        public ReadOnlyImageBuffer(ReadOnlyImageBuffer<T> existing, ReadOnlySpan<T> buffer)
        {
            Width = existing.Width;
            Height = existing.Height;
            Stride = existing.Stride;
            Buffer = buffer;
        }

        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public ReadOnlySpan<T> Buffer { get; }
        public ReadOnlySpan<T> GetRow(int row) => Buffer.Slice(Stride * row, Width);
    }
}