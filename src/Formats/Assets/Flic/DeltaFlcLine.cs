using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace UAlbion.Formats.Assets.Flic
{
    public class DeltaFlcLine
    {
        public override string ToString() =>
            $"Line [ {string.Join("; ", Tokens.Select(x => x.ToString()))} ]";

        public ushort Skip { get; }
        public byte? LastPixel { get; }
        public IList<DeltaFlcLineToken> Tokens { get; } = new List<DeltaFlcLineToken>();

        public DeltaFlcLine(BinaryReader br)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            int remaining = 1;
            while (remaining > 0)
            {
                var raw = br.ReadUInt16();
                var opcode = (RleOpcode)(raw >> 14);
                remaining--;

                switch (opcode)
                {
                    case RleOpcode.Packets:
                        Tokens = new DeltaFlcLineToken[raw];
                        for (int i = 0; i < raw; i++)
                            Tokens[i] = new DeltaFlcLineToken(br);
                        break;
                    case RleOpcode.StoreLowByteInLastPixel:
                        LastPixel = (byte)(0xff & raw);
                        remaining++;
                        break;
                    case RleOpcode.LineSkipCount:
                        Skip = (ushort)-raw;
                        remaining++;
                        break;
                    default: throw new InvalidEnumArgumentException(nameof(opcode), (int)opcode, typeof(RleOpcode));
                }
            }
        }
    }
}
