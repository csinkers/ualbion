using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UAlbion.Formats
{
    public class AlbionPalette
    {
        public class PaletteContext
        {
            public PaletteContext(int id, byte[] commonPalette) { Id = id; CommonPalette = commonPalette; }
            public PaletteContext(int id, string name, byte[] commonPalette) { Id = id; CommonPalette = commonPalette; Name = name; }
            public int Id { get; }
            public string Name { get; }
            public byte[] CommonPalette { get; }
        }

        public int Id { get; }
        public string Name { get; }
        public readonly uint[] Entries = new uint[0x100];
        public static readonly IDictionary<int, IList<(byte, byte)>> AnimatedRanges = new Dictionary<int, IList<(int, int)>> {
            { 0,  new[] { (0x99, 0x9f), (0xb0, 0xbf) } }, // 7, 16 => 112
            { 1,  new[] { (0x99, 0x9f), (0xb0, 0xb4), (0xb5, 0xbf) } }, // 7, 5, 11 => 385
            { 2,  new[] { (0x40, 0x43), (0x44, 0x4f) } }, // 4, 12 => 12
            { 5,  new[] { (0xb0, 0xb4), (0xb5, 0xbf) } }, // 5, 11 => 55
            { 13, new[] { (0xb0, 0xb3), (0xb4, 0xbf) } }, // 4, 12 => 12
            { 14, new[] { (0x58, 0x5f) } }, // 14 => 14
            { 24, new[] { (0xb0, 0xb3), (0xb4, 0xbf) } }, // 4, 12 => 12
            { 25, new[] { (0xb4, 0xb7), (0xb8, 0xbb), (0xbc, 0xbf) } }, // 4, 11, 4 => 44
            { 30, new[] { (0x10, 0x4f) } }, // 80 => 80
            { 50, new[] { (0xb0, 0xb3), (0xb4, 0xbf) } }, // 4, 12 => 12
        }.ToDictionary(
            x => x.Key, 
            x => (IList<(byte,byte)>)x.Value.Select(y => ((byte)y.Item1, (byte)y.Item2)).ToArray());

        public AlbionPalette(BinaryReader br, int streamLength, PaletteContext context)
        {
            Id = context.Id;
            Name = context.Name;
            Debug.Assert(context.CommonPalette.Length == 192);
            long startingOffset = br.BaseStream.Position;
            for (int i = 0; i < 192; i++)
            {
                Entries[i]  = (uint)br.ReadByte() << 24;
                Entries[i] |= (uint)br.ReadByte() << 16;
                Entries[i] |= (uint)br.ReadByte() << 8;
            }

            for (int i = 192; i < 256; i++)
            {
                Entries[i]  = (uint)context.CommonPalette[(i - 192) * 3 + 0] << 24;
                Entries[i] |= (uint)context.CommonPalette[(i - 192) * 3 + 1] << 16;
                Entries[i] |= (uint)context.CommonPalette[(i - 192) * 3 + 2] << 8;
            }

            Debug.Assert(br.BaseStream.Position == startingOffset + streamLength);
        }

        public AlbionPalette(BinaryReader br)
        {
            Id = -1;
            long startingOffset = br.BaseStream.Position;
            for (int i = 0; i < 256; i++)
            {
                Entries[i]  = (uint)br.ReadByte() << 24;
                Entries[i] |= (uint)br.ReadByte() << 16;
                Entries[i] |= (uint)br.ReadByte() << 8;
                br.ReadByte();
            }
        }

        public uint[] GetPaletteAtTime(int tick)
        {
            var result = new uint[256];
            AnimatedRanges.TryGetValue(Id, out var ranges);
            ranges = ranges ?? new List<(byte, byte)>();

            for (int i = 0; i < Entries.Length; i++)
            {
                int index = i;
                foreach (var range in ranges)
                {
                    if (i >= range.Item1 && i <= range.Item2)
                    {
                        index = (i - range.Item1 + tick) % (range.Item2 - range.Item1 + 1) + range.Item1;
                        break;
                    }
                }

                result[i] = Entries[index];
            }
            return result;
        }

        public override string ToString() { return string.IsNullOrEmpty(Name) ? $"Palette {Id}" : Name; }
    }
}
