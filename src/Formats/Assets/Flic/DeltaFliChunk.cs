using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic
{
    public class DeltaFliChunk : FlicChunk
    {
        public override FlicChunkType Type => FlicChunkType.DeltaByteOrientedRle;
        ushort LinesToSkip;
        ushort EncodedLines;

        struct Line
        {
            byte NumberOfPackets;
            byte LineSkipCount;

            struct Packet
            {
                byte SkipCount;
                byte PacketType;
                byte[] PixelData;
            }

            Packet[] Packets;
        }

        Line[] Lines;

        protected override uint SerdesBody(uint length, ISerializer s)
        {
            throw new NotImplementedException();
        }
    }
}