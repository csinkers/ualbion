using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Api;

namespace UAlbion.Formats.Assets
{
    public class AlbionPalette : IPalette
    {
        public int Id { get; }
        public string Name { get; }
        public bool IsAnimated => AnimatedRanges.ContainsKey(Id);
        public int Period { get; }
        readonly uint[] _entries = new uint[0x100];
        readonly IList<uint[]> _cache = new List<uint[]>();

        static readonly IDictionary<int, IList<(byte, byte)>> AnimatedRanges = new Dictionary<int, IList<(int, int)>> {
            { 0,  new[] { (0x99, 0x9f), (0xb0, 0xbf) } }, // 7, 16 => 112 // Outdoors Green (first island)
            { 1,  new[] { (0x99, 0x9f), (0xb0, 0xb4), (0xb5, 0xbf) } }, // 7, 5, 11 => 385 // Outdoors Green (slightly brighter, used for second island)
            { 2,  new[] { (0x40, 0x43), (0x44, 0x4f) } }, // 4, 12 => 12
            { 5,  new[] { (0xb0, 0xb4), (0xb5, 0xbf) } }, // 5, 11 => 55
            { 13, new[] { (0xb0, 0xb3), (0xb4, 0xbf) } }, // 4, 12 => 12
            { 14, new[] { (0x58, 0x5f) } }, // 14 => 14
            { 24, new[] { (0xb0, 0xb3), (0xb4, 0xbf) } }, // 4, 12 => 12
            { 25, new[] { (0xb4, 0xb7), (0xb8, 0xbb), (0xbc, 0xbf) } }, // 4, 11, 4 => 44
            { 30, new[] { (0x10, 0x4f) } }, // 80 => 80
            { 46,  new[] { (0x99, 0x9f), (0xb0, 0xb4), (0xb5, 0xbf) } }, // 7, 5, 11 => 385 // Outdoors Green - Night
            { 48, new[] { (0xb0, 0xb3), (0xb4, 0xbf) } }, // 4, 12 => 12 // Night-palette for 24
            { 50, new[] { (0xb0, 0xb3), (0xb4, 0xbf) } }, // 4, 12 => 12 // Dusk-palette for 24
            { 54,  new[] { (0x40, 0x43), (0x44, 0x4f) } }, // 4, 12 => 12 // Night-palette for 2
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
                _entries[i]  = (uint)br.ReadByte() << 0; // Red
                _entries[i] |= (uint)br.ReadByte() << 8; // Green
                _entries[i] |= (uint)br.ReadByte() << 16; // Blue
                _entries[i] |= (uint)(i == 0 ? 0 : 0xff) << 24; // Alpha
            }

            Debug.Assert(br.BaseStream.Position == startingOffset + streamLength);

            AnimatedRanges.TryGetValue(Id, out var ranges);
            ranges ??= new List<(byte, byte)>();
            Period = (int)ApiUtil.LCM(ranges.Select(x => (long)(x.Item2 - x.Item1 + 1)).Append(1));

            for (int cacheIndex = 0; cacheIndex < Period; cacheIndex++)
            {
                var result = new uint[256];
                for (int i = 0; i < _entries.Length; i++)
                {
                    int index = i;
                    foreach (var range in ranges)
                    {
                        if (i >= range.Item1 && i <= range.Item2)
                        {
                            int period = (range.Item2 - range.Item1 + 1);
                            int tickModulo = cacheIndex % period;
                            index = (i - range.Item1 + tickModulo) % period + range.Item1;
                            break;
                        }
                    }

                    result[i] = _entries[index];
                }
                _cache.Add(result);
            }
        }

        public void SetCommonPalette(byte[] commonPalette)
        {
            if (commonPalette == null) throw new ArgumentNullException(nameof(commonPalette));
            Debug.Assert(commonPalette.Length == 192);

            for (int i = 192; i < 256; i++)
            {
                _entries[i]  = (uint)commonPalette[(i - 192) * 3 + 0] << 0; // Red
                _entries[i] |= (uint)commonPalette[(i - 192) * 3 + 1] << 8; // Green
                _entries[i] |= (uint)commonPalette[(i - 192) * 3 + 2] << 16; // Blue
                _entries[i] |= (uint)0xff << 24; // Alpha

                foreach (var frame in _cache)
                    frame[i] = _entries[i];
            }
        }

        public IList<uint[]> GetCompletePalette() => _cache;
        public uint[] GetPaletteAtTime(int tick)
        {
            tick = int.MaxValue - tick;
            int index = tick % Period;
            return _cache[index];
        }

        public override string ToString() { return string.IsNullOrEmpty(Name) ? $"Palette {Id}" : $"{Name} ({Id})"; }
    }
}
