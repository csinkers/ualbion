using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Flic
{
    public class FlicFrame : FlicChunk
    {
        readonly int _videoWidth;
        readonly int _videoHeight;
        const int HeaderSize = 16;

        public FlicFrame(int width, int height)
        {
            _videoWidth = width;
            _videoHeight = height;
        }

        public override FlicChunkType Type => FlicChunkType.Frame;
        public IList<FlicChunk> SubChunks { get; } = new List<FlicChunk>();
        public ushort Delay { get; private set; }
        protected override uint SerdesBody(uint length, ISerializer s)
        {
            var initialOffset = s.Offset;
            ushort subChunkCount = s.UInt16("SubChunkCount", (ushort)SubChunks.Count);
            Delay = s.UInt16(nameof(Delay), Delay);
            s.UInt16("reserved", 0);
            Width = s.UInt16(nameof(Width), Width);
            Height = s.UInt16(nameof(Height), Height);

            s.List(nameof(SubChunks), SubChunks, subChunkCount,
                (i, c, s2) => Serdes(i, c, s2, _videoWidth, _videoHeight));

            if (s.Mode == SerializerMode.Reading)
                ApiUtil.Assert(s.Offset == initialOffset + length);
            return length;
        }

        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
    }
}