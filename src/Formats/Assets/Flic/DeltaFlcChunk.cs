using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UAlbion.Formats.Assets.Flic
{
    public class DeltaFlcChunk : FlicChunk
    {
        public override FlicChunkType Type => FlicChunkType.DeltaWordOrientedRle;

        public enum RleOpcode : byte
        {
            Packets = 0,
            Undefined = 1,
            StoreLowByteInLastPixel = 2,
            LineSkipCount = 3, // Take absolute value first
        }

        public class LineToken
        {
            public override string ToString()
                    => $"LineToken:Skip{ColumnSkipCount}:{(SignedCount > 0 ? $"Lit{SignedCount}" : $"Rle{-SignedCount}")}[ "
                    + string.Join(", ", PixelData.Select(x => $"{x}"))
                    + " ]";

            public byte ColumnSkipCount { get; }
            public sbyte SignedCount { get; }
            public ushort[] PixelData { get; }

            public LineToken(BinaryReader br)
            {
                StartedAt = br.BaseStream.Position;
                ColumnSkipCount = br.ReadByte();
                SignedCount = br.ReadSByte(); // +ve = verbatim, -ve = RLE

                if (SignedCount > 0)
                {
                    PixelData ??= new ushort[SignedCount];
                    for (int j = 0; j < SignedCount; j++)
                        PixelData[j] = br.ReadUInt16();
                }
                else
                {
                    PixelData ??= new ushort[1];
                    PixelData[0] = br.ReadUInt16();
                }

                BytesRead = (ushort)(br.BaseStream.Position - StartedAt);
            }

            public long StartedAt { get; }
            public ushort BytesRead { get; }
        }

        public class Line
        {
            public override string ToString() => 
                $"Line [ {string.Join("; ", Tokens.Select(x => x.ToString()))} ]";

            public ushort Skip { get; }
            public byte? LastPixel { get; }
            public IList<LineToken> Tokens { get; } = new List<LineToken>();

            public Line(BinaryReader br)
            {
                StartedAt = br.BaseStream.Position;

                int remaining = 1;
                while (remaining > 0)
                {
                    var raw = br.ReadUInt16();
                    var opcode = (RleOpcode)(byte)(raw >> 14);
                    remaining--;

                    switch (opcode)
                    {
                        case RleOpcode.Packets:
                            Tokens = new LineToken[raw];
                            for (int i = 0; i < raw; i++)
                                Tokens[i] = new LineToken(br);
                            break;
                        case RleOpcode.StoreLowByteInLastPixel:
                            LastPixel = (byte)(0xff & raw);
                            remaining++;
                            break;
                        case RleOpcode.LineSkipCount:
                            Skip = (ushort)-raw;
                            remaining++;
                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }

                BytesRead = (ushort)(br.BaseStream.Position - StartedAt);
            }

            public long StartedAt { get; }
            public ushort BytesRead { get; }
        }

        public Line[] Lines { get; private set; }
        public long StartedAt { get; private set; }
        public ushort BytesRead { get; private set; }

        protected override uint LoadChunk(uint length, BinaryReader br)
        {
            StartedAt = br.BaseStream.Position;
            ushort lineCount = br.ReadUInt16();
            Lines ??= new Line[lineCount];
            for(int i = 0; i < lineCount; i++)
                Lines[i] = new Line(br);
            BytesRead = (ushort)(br.BaseStream.Position - StartedAt);
            return BytesRead;
        }

        public void Apply(byte[] buffer8, int width)
        {
            int y = 0;
            int x = 0;
            void Write(byte b)
            {
                buffer8[y * width + x] = b;
                x++;
            }

            foreach(var line in Lines)
            {
                y += line.Skip;
                foreach (var token in line.Tokens)
                {
                    x += token.ColumnSkipCount;
                    if(token.SignedCount > 0)
                    {
                        for (int i = 0; i < token.SignedCount; i++)
                        {
                            Write((byte)(token.PixelData[i] & 0xff));
                            Write((byte)((token.PixelData[i] & 0xff00) >> 8));
                        }
                    }
                    else // RLE
                    {
                        for (int i = 0; i < -token.SignedCount; i++)
                        {
                            Write((byte)(token.PixelData[0] & 0xff));
                            Write((byte)((token.PixelData[0] & 0xff00) >> 8));
                        }
                    }
                }

                x = 0;
                y++;
            }
        }
    }
}
