using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UAlbion.Formats
{
    public class AlbionPalette
    {
        public int Id { get; }
        public string Name { get; }
        public bool IsAnimated => AnimatedRanges.ContainsKey(Id);
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
            x => (IList<(byte, byte)>)x.Value.Select(y => ((byte)y.Item1, (byte)y.Item2)).ToArray());

        public AlbionPalette(BinaryReader br, int streamLength, string name, int id)
        {
            Id = id;
            Name = name;
            long startingOffset = br.BaseStream.Position;
            for (int i = 0; i < 192; i++)
            {
                //*
                Entries[i]  = (uint)br.ReadByte() << 0; // Red
                Entries[i] |= (uint)br.ReadByte() << 8; // Green
                Entries[i] |= (uint)br.ReadByte() << 16; // Blue
                Entries[i] |= (uint)(i == 0 ? 0 : 0xff) << 24; // Alpha
                //*/
                /*
                br.ReadBytes(3);
                Entries[i] = 0;
                Entries[i] |= (uint)0xff << 24; // Alpha
                Entries[i] |= (uint)i << 16; // Blue
                Entries[i] |= (uint)i << 8; // Green
                Entries[i] |= (uint)i << 0; // Red
                //*/
            }

            Debug.Assert(br.BaseStream.Position == startingOffset + streamLength);
        }
        public void SetCommonPalette(byte[] commonPalette)
        {
            if (commonPalette == null) throw new ArgumentNullException(nameof(commonPalette));
            Debug.Assert(commonPalette.Length == 192);

            for (int i = 192; i < 256; i++)
            {
                //*
                Entries[i]  = (uint)commonPalette[(i - 192) * 3 + 0] << 0; // Red
                Entries[i] |= (uint)commonPalette[(i - 192) * 3 + 1] << 8; // Green
                Entries[i] |= (uint)commonPalette[(i - 192) * 3 + 2] << 16; // Blue
                Entries[i] |= (uint)0xff << 24; // Alpha
                //*/
                /*
                Entries[i] = 0;
                Entries[i] |= (uint)i << 16;
                Entries[i] |= (uint)i << 8;
                Entries[i] |= (uint)i << 0;
                Entries[i] |= (uint)0xff << 24; // Alpha
                //*/
            }
        }

        public uint[] GetPaletteAtTime(int tick)
        {
            var result = new uint[256];
            AnimatedRanges.TryGetValue(Id, out var ranges);
            ranges = ranges ?? new List<(byte, byte)>();
            tick = int.MaxValue - tick;

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

        public override string ToString() { return string.IsNullOrEmpty(Name) ? $"Palette {Id}" : $"{Name} ({Id})"; }
    }
}
