using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;

namespace UAlbion.Formats.Assets;

public class AlbionPalette : IPalette
{
    const int EntryCount = 256;
    const int CommonEntries = 64;
    const int VariableEntries = EntryCount - CommonEntries;

    readonly int[] _periods = new int[EntryCount];
    readonly IList<(byte, byte)> _ranges = new List<(byte, byte)>();
    SimpleTexture<uint> _texture;
    uint[] _unambiguous;

    public uint Id { get; private set; }
    public string Name { get; private set; }
    [JsonInclude] public uint[] Entries { get; private set; } = new uint[EntryCount];
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

            _texture = new SimpleTexture<uint>(AssetId.None, 256, totalPeriod);
            var span = _texture.GetMutableLayerBuffer(0).Buffer;
            int offset = 0;
            for (int cacheIndex = 0; cacheIndex < totalPeriod; cacheIndex++)
            {
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

                    span[offset++] = Entries[index];
                }

                _texture.AddRegion(0, cacheIndex, 256, 1);
            }

            for (int i = 0; i < _periods.Length; i++)
                _periods[i] = 1;

            foreach (var range in _ranges)
                for (int i = range.Item1; i <= range.Item2; i++)
                    _periods[i] = range.Item2 - range.Item1 + 1;

            Period = totalPeriod;
        }
    }

    [JsonIgnore]
    public IEnumerable<(byte, int)> AnimatedEntries =>
        from r in Ranges
        let rangeLength = r.Item2 - r.Item1 + 1
        from e in Enumerable.Range(r.Item1, rangeLength)
        select ((byte)e, rangeLength);

    public int CalculatePeriod(IEnumerable<byte> distinctColors)
        => (int)ApiUtil.Lcm(distinctColors.Select(x => (long)_periods[x]).Append(1));

    public void SetCommonPalette(AlbionPalette commonPalette)
    {
        if (commonPalette == null)
            throw new ArgumentNullException(nameof(commonPalette));

        var span = _texture.GetMutableLayerBuffer(0).Buffer;
        for (int offset = 0; offset < span.Length; offset += 256)
        {
            for (int i = VariableEntries; i < EntryCount; i++)
            {
                Entries[i] = commonPalette.Entries[i];
                span[offset + i] = Entries[i];
            }
        }
    }

    [JsonIgnore] public IReadOnlyTexture<uint> Texture => _texture;

    static uint Search(uint root, ISet<uint> visited)
    {
        var queue = new Queue<uint>();
        queue.Enqueue(root);
        do
        {
            var entry = queue.Dequeue();
            if (!visited.Contains(entry))
                return entry;

            var (r, g, b, a) = ApiUtil.UnpackColor(entry);
            if (r < 255) queue.Enqueue(ApiUtil.PackColor((byte)(r + 1), g, b, a));
            if (g < 255) queue.Enqueue(ApiUtil.PackColor(r, (byte)(g + 1), b, a));
            if (b < 255) queue.Enqueue(ApiUtil.PackColor(r, g, (byte)(b + 1), a));
            if (r > 0) queue.Enqueue(ApiUtil.PackColor((byte)(r - 1), g, b, a));
            if (g > 0) queue.Enqueue(ApiUtil.PackColor(r, (byte)(g - 1), b, a));
            if (b > 0) queue.Enqueue(ApiUtil.PackColor(r, g, (byte)(b - 1), a));
        } while (queue.Count > 0);

        throw new InvalidOperationException($"Could not find an empty palette slot for {root:x}");
    }

    public uint[] GetUnambiguousPalette()
    {
        if (_unambiguous == null)
        {
            _unambiguous = new uint[256];
            var used = new HashSet<uint>();
            for (int i = 0; i < 256; i++)
            {
                uint entry = Entries[i];

                // Always set full alpha when it's not index 0 so things like item graphics
                // that should just use the common palette still round-trip correctly if 
                // they accidentally contain some indices < 192
                if (i > 0)
                    entry |= 0xff000000; 

                if (used.Contains(entry))
                    entry = Search(entry, used);

                _unambiguous[i] = entry;
                used.Add(entry);
            }
        }

        return _unambiguous;
    }

    public ReadOnlySpan<uint> GetPaletteAtTime(int i) => _texture.GetLayerBuffer(0).GetRow(i % _texture.Height);
    public override string ToString() { return string.IsNullOrEmpty(Name) ? $"Palette {Id}" : $"{Name} ({Id})"; }

    public static AlbionPalette Serdes(AlbionPalette p, AssetInfo info, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (info == null) throw new ArgumentNullException(nameof(info));

        bool isCommon = info.Get(AssetProperty.IsCommon, false);
        long entryCount = isCommon ? CommonEntries : VariableEntries;

        if (p == null)
        {
            if (s.IsWriting()) throw new ArgumentNullException(nameof(p));

            // AssetId is None when loading palettes from raw data in ImageReverser
            p = new AlbionPalette
            {
                Id = info.AssetId.IsNone ? (uint)info.Index : info.AssetId.ToUInt32(),
                Name = info.AssetId.IsNone 
                    ? info.Index.ToString(CultureInfo.InvariantCulture) 
                    : info.AssetId.ToString()
            };
        }

        if (s.IsReading() && s.BytesRemaining != entryCount * 3)
            throw new InvalidDataException($"Palette had invalid size {s.BytesRemaining}, expected {entryCount * 3}");

        for (int i = isCommon ? VariableEntries : 0; i < (isCommon ? EntryCount : VariableEntries); i++)
        {
            var (r, g, b, _) = ApiUtil.UnpackColor(p.Entries[i]);

            r = s.UInt8(null, r); // Red
            g = s.UInt8(null, g); // Green
            b = s.UInt8(null, b); // Blue
            var a = (byte)(i == 0 ? 0 : 0xff); // Alpha

            p.Entries[i] = ApiUtil.PackColor(r, g, b, a);
        }

        if (s.IsReading())
            p.Ranges = ParseRanges(info);

        return p;
    }

    static IEnumerable<(byte, byte)> ParseRanges(AssetInfo info) =>
        info.Get<string>(AssetProperty.AnimatedRanges, null)?.Split(',').Select(x =>
        {
            var parts = x.Trim().Split('-');
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