using System;
using System.IO;
using System.Linq;

namespace UAlbion.Formats.Assets.Flic
{
    public class DeltaFlcLineToken
    {
        public override string ToString()
            => $"LineToken:Skip{ColumnSkipCount}:{(SignedCount > 0 ? $"Lit{SignedCount}" : $"Rle{-SignedCount}")}[ "
               + string.Join(", ", PixelData.Select(x => $"{x}"))
               + " ]";

        public byte ColumnSkipCount { get; }
        public sbyte SignedCount { get; }
        public ushort[] PixelData { get; }

        public DeltaFlcLineToken(BinaryReader br)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
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
        }
    }
}
