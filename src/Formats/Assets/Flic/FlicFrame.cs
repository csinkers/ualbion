using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Flic
{
    public class FlicFrame : FlicChunk
    {
        readonly int _videoWidth;
        readonly int _videoHeight;

        public FlicFrame(int width, int height)
        {
            _videoWidth = width;
            _videoHeight = height;
        }

        public override FlicChunkType Type => FlicChunkType.Frame;
        public IList<FlicChunk> SubChunks { get; } = new List<FlicChunk>();
        public ushort Delay { get; private set; }
        public ushort Width { get; private set; } // Overrides, usually 0.
        public ushort Height { get; private set; }

        protected override uint LoadChunk(uint length, BinaryReader br)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            var initialOffset = br.BaseStream.Position;
            ushort subChunkCount = br.ReadUInt16();
            Delay = br.ReadUInt16();
            br.ReadUInt16();
            Width = br.ReadUInt16();
            Height = br.ReadUInt16();

            for (int i = 0; i < subChunkCount; i++)
                SubChunks.Add(Load(br, _videoWidth, _videoHeight));

            ApiUtil.Assert(br.BaseStream.Position == initialOffset + length);
            return length;
        }
    }
}
