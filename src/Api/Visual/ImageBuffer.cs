using System;

namespace UAlbion.Api.Visual
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Won't be compared")]
    public readonly ref struct ImageBuffer<T> where T : unmanaged
    {
        public ImageBuffer(int width, int height, int stride, Span<T> buffer)
        {
            Width = width;
            Height = height;
            Stride = stride;
            Buffer = buffer;
        }

        public ImageBuffer(ImageBuffer<T> existing, Span<T> buffer)
        {
            Width = existing.Width;
            Height = existing.Height;
            Stride = existing.Stride;
            Buffer = buffer;
        }

        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public Span<T> Buffer { get; }
        public Span<T> GetRow(int row) => Buffer.Slice(Stride * row, Width);
    }
}
