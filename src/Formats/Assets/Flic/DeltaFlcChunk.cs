using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic
{
    public class DeltaFlcChunk : FlicChunk
    {
        public override FlicChunkType Type => FlicChunkType.DeltaWordOrientedRle;

        public enum RleOpcode : byte
        {
            PacketCount = 0,
            Undefined = 1,
            StoreLowByteInLastPixel = 2,
            LineSkipCount = 3, // Take absolute value first
        }

        public class Line
        {
            public ushort PacketCount { get; private set; }

            public class Packet
            {
                public byte ColumnSkipCount { get; private set; }
                public sbyte RleCount { get; private set; }
                public ushort[] PixelData { get; private set; }

                public static Packet Serdes(int i, Packet p, ISerializer s)
                {
                    p ??= new Packet();
                    var startOffset = s.Offset;
                    p.ColumnSkipCount = s.UInt8(nameof(ColumnSkipCount), p.ColumnSkipCount);
                    p.RleCount = s.Int8(nameof(RleCount), p.RleCount); // +ve = verbatim, -ve = RLE

                    if (p.RleCount > 0)
                    {
                        p.PixelData ??= new ushort[p.RleCount];
                        for(int j = 0; j < p.RleCount; j++)
                            p.PixelData[j] = s.UInt16(null, p.PixelData[j]);
                    }
                    else
                    {
                        p.PixelData ??= new ushort[1];
                        p.PixelData[0] = s.UInt16(nameof(PixelData), p.PixelData[0]);
                    }

                    p.BytesRead = (ushort)(s.Offset - startOffset);
                    return p;
                }

                public ushort BytesRead { get; private set; }
            }

            public Packet[] Packets;

            public static Line Serdes(int i, Line l, ISerializer s)
            {
                l ??= new Line();
                var startOffset = s.Offset;
                l.PacketCount = s.UInt16(nameof(PacketCount), l.PacketCount);
                RleOpcode opcode = (RleOpcode)(byte)(l.PacketCount >> 14);

                switch(opcode)
                {
                    case RleOpcode.PacketCount:
                        l.Packets ??= new Packet[l.PacketCount];
                        s.List(nameof(Packets), l.Packets, l.PacketCount, Packet.Serdes);
                        break;
                    case RleOpcode.StoreLowByteInLastPixel: break;
                    case RleOpcode.LineSkipCount: break;
                    default: throw new ArgumentOutOfRangeException();
                }

                l.BytesRead = (ushort)(s.Offset - startOffset);
                return l;
            }

            public ushort BytesRead { get; private set; }
        }

        public Line[] Lines { get; private set; }
        public ushort BytesRead { get; private set; }

        protected override uint SerdesBody(uint length, ISerializer s)
        {
            var startOffset = s.Offset;
            ushort lineCount = s.UInt16("LineCount", (ushort)(Lines?.Length ?? 0));
            Lines ??= new Line[lineCount];
            s.List(nameof(Lines), Lines, lineCount, Line.Serdes);
            BytesRead = (ushort)(s.Offset - startOffset);
            return (uint)(s.Offset - startOffset);
        }
    }
}