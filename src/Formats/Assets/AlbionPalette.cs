using System;
using System.Collections.Generic;
using System.IO;
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

        readonly int[] _periods = new int[EntryCount];
        readonly IList<uint[]> _cache = new List<uint[]>();
        readonly IList<(byte, byte)> _ranges = new List<(byte, byte)>();

        public uint Id { get; private set; }
        public string Name { get; private set; }
        public uint[] Entries { get; private set; } = new uint[EntryCount];
        [JsonIgnore] public bool IsAnimated => Period > 1;
        [JsonIgnore] public int Period { get; private set; }

        public AlbionPalette() { }

        public AlbionPalette(
            uint id,
            string name,
            uint[] entries,
            IList<(byte, byte)> ranges = null)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            if (entries.Length != EntryCount)
                throw new ArgumentOutOfRangeException(nameof(entries), $"Expected entries to be an array of {EntryCount} elements");

            Id = id;
            Name = name;
            Array.Copy(entries, Entries, EntryCount);
            Ranges = ranges;
        }

        public IEnumerable<(byte, byte)> Ranges
        {
            get => _ranges;
            set
            {
                if (ReferenceEquals(value, _ranges))
                    return;

                _ranges.Clear();
                if (value != null)
                    foreach (var range in value)
                        _ranges.Add(range);

                var totalPeriod = (int)ApiUtil.Lcm(_ranges
                    .Select(x => (long)(x.Item2 - x.Item1 + 1))
                    .Append(1));

                _cache.Clear();
                for (int cacheIndex = 0; cacheIndex < totalPeriod; cacheIndex++)
                {
                    var result = new uint[256];
                    for (int i = 0; i < EntryCount; i++)
                    {
                        int index = i;
                        foreach (var range in _ranges)
                        {
                            if (i >= range.Item1 && i <= range.Item2)
                            {
                                int period = range.Item2 - range.Item1 + 1;
                                int tickModulo = cacheIndex % period;
                                index = (i - range.Item1 + tickModulo) % period + range.Item1;
                                break;
                            }
                        }

                        result[i] = Entries[index];
                    }
                    _cache.Add(result);
                }

                for (int i = 0; i < _periods.Length; i++)
                    _periods[i] = 1;

                foreach (var range in _ranges)
                    for (int i = range.Item1; i <= range.Item2; i++)
                        _periods[i] = range.Item2 - range.Item1 + 1;

                Period = totalPeriod;
            }
        }

        public int CalculatePeriod(IEnumerable<byte> distinctColors)
            => (int)ApiUtil.Lcm(distinctColors.Select(x => (long)_periods[x]).Append(1));

        public void SetCommonPalette(AlbionPalette commonPalette)
        {
            if (commonPalette == null)
                throw new ArgumentNullException(nameof(commonPalette));

            for (int i = VariableEntries; i < EntryCount; i++)
            {
                Entries[i] = commonPalette.Entries[i];
                foreach (var frame in _cache)
                    frame[i] = Entries[i];
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

        public static AlbionPalette Serdes(AlbionPalette p, AssetInfo info, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (info == null) throw new ArgumentNullException(nameof(info));

            bool isCommon = info.Get("IsCommon", false);
            long entryCount = isCommon ? CommonEntries : VariableEntries;

            if (p == null)
            {
                if (s.IsWriting()) throw new ArgumentNullException(nameof(p));

                // AssetId is None when loading palettes from raw data in ImageReverser
                p = new AlbionPalette
                {
                    Id = info.AssetId.IsNone ? (uint) info.SubAssetId : info.AssetId.ToUInt32(),
                    Name = info.AssetId.IsNone ? info.Name : info.AssetId.ToString()
                };
            }

            if (s.IsReading() && s.BytesRemaining != entryCount * 3)
                throw new InvalidDataException($"Palette had invalid size {s.BytesRemaining}, expected {entryCount * 3}");

            for (int i = isCommon ? VariableEntries : 0; i < (isCommon ? EntryCount : VariableEntries); i++)
            {
                var (r, g, b, _) = FormatUtil.UnpackColor(p.Entries[i]);

                r = s.UInt8(null, r); // Red
                g = s.UInt8(null, g); // Green
                b = s.UInt8(null, b); // Blue
                var a = (byte)(i == 0 ? 0 : 0xff); // Alpha

                p.Entries[i] = FormatUtil.PackColor(r, g, b, a);
            }

            if (s.IsReading())
                p.Ranges = ParseRanges(info);

            return p;
        }

        static IEnumerable<(byte, byte)> ParseRanges(AssetInfo info) =>
            info.GetArray<string>("AnimatedRanges")?.Select(x =>
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
            });
    }
}
