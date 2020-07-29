using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Flic
{
    public abstract class FlicChunk
    {
        const uint ChunkHeaderSize = 6;
        public abstract FlicChunkType Type { get; }
        protected abstract uint LoadChunk(uint length, BinaryReader br);

        // Need w & h to fixup issue in CopyChunk size
        public static FlicChunk Load(BinaryReader br, int width, int height) 
        {
            var chunkSizeOffset = br.BaseStream.Position;
            uint chunkSize = br.ReadUInt32();
            if ((chunkSize & 0x1) != 0)
                chunkSize++;
            FlicChunkType type = (FlicChunkType)br.ReadUInt16();

            // Chunk
            FlicChunk c = type switch
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

            // Fix export issue in Autodesk Animator
            if (type == FlicChunkType.FullUncompressed &&
                width * height + ChunkHeaderSize - 2 == chunkSize)
            {
                chunkSize += 2;
            }

            chunkSize = c.LoadChunk(chunkSize - ChunkHeaderSize, br) + ChunkHeaderSize;

            var actualChunkSize = br.BaseStream.Position - chunkSizeOffset;
            ApiUtil.Assert(actualChunkSize == chunkSize);
            return c;
        }
    }
}
