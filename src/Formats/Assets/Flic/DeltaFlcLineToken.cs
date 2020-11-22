using System;
using System.Linq;
using SerdesNet;

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

        public DeltaFlcLineToken(ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            ColumnSkipCount = s.UInt8(null, 0);
            SignedCount = s.Int8(null, 0); // +ve = verbatim, -ve = RLE

            if (SignedCount > 0)
            {
                PixelData ??= new ushort[SignedCount];
                for (int j = 0; j < SignedCount; j++)
                    PixelData[j] = s.UInt16(null, 0);
            }
            else
            {
                PixelData ??= new ushort[1];
                PixelData[0] = s.UInt16(null, 0);
            }
        }
    }
}
