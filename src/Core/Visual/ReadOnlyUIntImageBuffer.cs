using System;

namespace UAlbion.Core.Visual
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Won't be compared")]
    public readonly ref struct ReadOnlyUIntImageBuffer
    {
        public ReadOnlyUIntImageBuffer(int width, int height, int stride, ReadOnlySpan<uint> buffer)
        {
            Width = width;
            Height = height;
            Stride = stride;
            Buffer = buffer;
        }

        public ReadOnlyUIntImageBuffer(ReadOnlyUIntImageBuffer existing, ReadOnlySpan<uint> buffer)
        {
            Width = existing.Width;
            Height = existing.Height;
            Stride = existing.Stride;
            Buffer = buffer;
        }

        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public ReadOnlySpan<uint> Buffer { get; }
    }
}