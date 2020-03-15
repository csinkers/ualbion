using System;

namespace UAlbion.Core
{
    public readonly ref struct ReadOnlyByteImageBuffer
    {
        public ReadOnlyByteImageBuffer(uint width, uint height, uint stride, ReadOnlySpan<byte> buffer)
        {
            Width = width;
            Height = height;
            Stride = stride;
            Buffer = buffer;
        }

        public uint Width { get; }
        public uint Height { get; }
        public uint Stride { get; }
        public ReadOnlySpan<byte> Buffer { get; }
    }
}