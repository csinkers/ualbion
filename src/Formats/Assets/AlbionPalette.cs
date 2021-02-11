using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    public class AlbionPalette : IPalette
    {
        const int EntryCount = 256;
        const int CommonEntries = 64;
        const int VariableEntries = EntryCount - CommonEntries;
        public uint Id { get; }
        public string Name { get; }
        [JsonIgnore] public bool IsAnimated => Period > 1;
        public int Period { get; }
        readonly int[] _periods = new int[EntryCount];
        readonly uint[] _entries = new uint[EntryCount];
        readonly IList<uint[]> _cache = new List<uint[]>();

        public AlbionPalette(
            uint id,
            string name,
            uint[] entries,
            IList<(byte, byte)> ranges = null)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            Id = id;
            Name = name;
            _entries = entries.ToArray();
            Period = InitRanges(ranges);
        }

        public AlbionPalette(ISerializer s, AssetInfo info)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (info == null) throw new ArgumentNullException(nameof(info));
            var streamLength = s.BytesRemaining;
            bool isCommon = streamLength == CommonEntries * 3;

            long startingOffset = s.Offset;
            for (int i = isCommon ? VariableEntries : 0; i < (isCommon ? EntryCount : VariableEntries); i++)
            {
                _entries[i] =        s.UInt8(null, 0);       // Red
                _entries[i] |= (uint)s.UInt8(null, 0) << 8;  // Green
                _entries[i] |= (uint)s.UInt8(null, 0) << 16; // Blue
                _entries[i] |= (uint)(i == 0 ? 0 : 0xff) << 24; // Alpha
            }

            ApiUtil.Assert(s.Offset == startingOffset + streamLength);

            var ranges = info.GetArray<string>("AnimatedRanges")?.Select(x =>
            {
                var parts = x.Split('-');
                if (parts.Length != 2)
                {
                    throw new InvalidOperationException(
                        "Palette animated ranges must be of the form \"FROM-TO\" " +
                        "where FROM and TO are either decimal or hex values between 0 and 255. e.g. \"0x23-0x4b\" or \"10-23\"). " +
                        $"The incorrect value was {x}");
                }

                return ((byte)FormatUtil.ParseHex(parts[0]), (byte)FormatUtil.ParseHex(parts[1]));
            }).ToList() ?? new List<(byte, byte)>();

            // AssetId is None when loading palettes from raw data in ImageReverser
            Id = info.AssetId.IsNone ? (uint)info.Id : info.AssetId.ToUInt32();
            Name = info.AssetId.IsNone ? info.Name : info.AssetId.ToString();
            Period = InitRanges(ranges);
        }

        int InitRanges(IList<(byte, byte)> ranges)
        {
            ranges ??= Array.Empty<(byte, byte)>();
            var totalPeriod = (int)ApiUtil.Lcm(ranges
                .Select(x => (long)(x.Item2 - x.Item1 + 1))
                .Append(1));

            for (int cacheIndex = 0; cacheIndex < totalPeriod; cacheIndex++)
            {
                var result = new uint[256];
                for (int i = 0; i < _entries.Length; i++)
                {
                    int index = i;
                    foreach (var range in ranges)
                    {
                        if (i >= range.Item1 && i <= range.Item2)
                        {
                            int period = range.Item2 - range.Item1 + 1;
                            int tickModulo = cacheIndex % period;
                            index = (i - range.Item1 + tickModulo) % period + range.Item1;
                            break;
                        }
                    }

                    result[i] = _entries[index];
                }
                _cache.Add(result);
            }

            for (int i = 0; i < _periods.Length; i++)
                _periods[i] = 1;

            foreach (var range in ranges)
                for (int i = range.Item1; i <= range.Item2; i++)
                    _periods[i] = range.Item2 - range.Item1 + 1;

            return totalPeriod;
        }

        public int CalculatePeriod(IEnumerable<byte> distinctColors)
            => (int)ApiUtil.Lcm(distinctColors.Select(x => (long)_periods[x]).Append(1));

        public void SetCommonPalette(AlbionPalette commonPalette)
        {
            if (commonPalette == null)
                throw new ArgumentNullException(nameof(commonPalette));

            for (int i = VariableEntries; i < EntryCount; i++)
            {
                _entries[i] = commonPalette._entries[i];
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
