using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Flic
{
    public abstract class FlicChunk
    {
        const uint ChunkHeaderSize = 6;
        public abstract FlicChunkType Type { get; }
        protected abstract uint SerdesBody(uint length, ISerializer s);

        // Need w & h to fixup issue in CopyChunk size
        public static FlicChunk Serdes(int i, FlicChunk c, ISerializer s, int width, int height) 
        {
            s.Begin("Chunk");
            var chunkSizeOffset = s.Offset;
            uint chunkSize = s.UInt32("ChunkSize", ChunkHeaderSize);
            if ((chunkSize & 0x1) != 0)
                chunkSize++;
            FlicChunkType type = s.EnumU16(nameof(Type), c?.Type ?? 0);

            // Chunk
            if (c == null)
            {
                c = type switch
                {
                    FlicChunkType.Palette8Bit          => new Palette8Chunk(),
                    FlicChunkType.DeltaWordOrientedRle => new DeltaFlcChunk(),
                    // FlicChunkType.Palette6Bit          => new Palette6Chunk(),
                    // FlicChunkType.DeltaByteOrientedRle => new DeltaFliChunk(),
                    // FlicChunkType.BlackFrameData       => new BlackFrameChunk(),
                    // FlicChunkType.FullByteOrientedRle  => new FullByteOrientedRleChunk(),
                    FlicChunkType.FullUncompressed     => new CopyChunk(),
                    // FlicChunkType.Thumbnail            => new ThumbnailChunk(),
                    FlicChunkType.Frame => new FlicFrame(width, height),
                    _ => new UnknownChunk(type)
                };
            }

            // Fix export issue in Autodesk Animator
            if (type == FlicChunkType.FullUncompressed &&
                width * height + ChunkHeaderSize - 2 == chunkSize)
            {
                chunkSize += 2;
            }

            chunkSize = c.SerdesBody(chunkSize - ChunkHeaderSize, s) + ChunkHeaderSize;

            var actualChunkSize = s.Offset - chunkSizeOffset;
            if (s.Mode == SerializerMode.Reading)
                ApiUtil.Assert(actualChunkSize == chunkSize);
            s.Seek(chunkSizeOffset);
            s.UInt32("ChunkSize", (uint)actualChunkSize);
            s.Seek(chunkSizeOffset + actualChunkSize);
            s.End();
            return c;
        }
    }
}
