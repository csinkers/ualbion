using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Flic
{
    public abstract class FlicChunk
    {
        const uint ChunkHeaderSize = 6;
        public abstract FlicChunkType Type { get; }
        protected abstract uint LoadChunk(uint length, ISerializer br);

        public static FlicChunk Load(ISerializer s, int width, int height) 
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            var chunkSizeOffset = s.Offset;
            uint chunkSize = s.UInt32(null, 0);
            if ((chunkSize & 0x1) != 0)
                chunkSize++;
            FlicChunkType type = (FlicChunkType)s.UInt16(null, 0);

            // Chunk
            FlicChunk c = type switch
            {
                FlicChunkType.Palette8Bit          => new Palette8Chunk(),
                FlicChunkType.DeltaWordOrientedRle => new DeltaFlcChunk(),
                FlicChunkType.FullByteOrientedRle  => new FullByteOrientedRleChunk(width, height),
                FlicChunkType.FullUncompressed     => new CopyChunk(),
                FlicChunkType.Frame                => new FlicFrame(width, height),
                // FlicChunkType.Palette6Bit          => new Palette6Chunk(),
                // FlicChunkType.DeltaByteOrientedRle => new DeltaFliChunk(),
                // FlicChunkType.BlackFrameData       => new BlackFrameChunk(),
                // FlicChunkType.Thumbnail            => new ThumbnailChunk(),
                _ => new UnknownChunk(type)
            };

            // Fix export issue in Autodesk Animator
            if (type == FlicChunkType.FullUncompressed &&
                width * height + ChunkHeaderSize - 2 == chunkSize)
            {
                chunkSize += 2;
            }

            chunkSize = c.LoadChunk(chunkSize - ChunkHeaderSize, s) + ChunkHeaderSize;

            var actualChunkSize = s.Offset - chunkSizeOffset;

            if (actualChunkSize - chunkSize < 4) // pad
            {
                for (long i = chunkSize - actualChunkSize; i != 0; i--)
                    s.UInt8(null, 0);

                actualChunkSize = s.Offset - chunkSizeOffset;
            }

            ApiUtil.Assert(actualChunkSize == chunkSize);
            return c;
        }
    }
}
