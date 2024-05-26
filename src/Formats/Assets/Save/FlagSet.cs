using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets.Save;

public class FlagSet
{
    readonly BitArray _set;
    public int BitsPerMap { get; }
    public int Count => _set.Count;
    public int PackedSize => (Count + 7) / 8;

    public IEnumerable<int> ActiveFlagIndices // Debugging convenience property
    {
        get
        {
            for (int i = 0; i < _set.Count; i++)
            {
                if (_set[i])
                    yield return i;
            }
        }
    }

    public FlagSet(int mapCount, int bitsPerMap)
    {
        BitsPerMap = bitsPerMap;
        _set = new BitArray(mapCount * bitsPerMap);
    }

    public FlagSet(int count)
    {
        BitsPerMap = 0;
        _set = new BitArray(count);
    }

    public bool this[int i]
    {
        get => i >= 0 && i < Count && _set[i];
        set
        {
            if(i < Count)
                _set[i] = value;
        }
    }

    public bool GetFlag(int i) => i >= 0 && i < Count && _set[i];
    public bool GetFlag(MapId mapId, int i) => GetFlag(mapId.Id * BitsPerMap + i);
    public void SetFlag(MapId mapId, int i, bool value)
    {
        if (i < 0)
        {
            ApiUtil.Assert($"Tried to set negative bit {i} for map {mapId}");
            return;
        }

        if (i > BitsPerMap)
        {
            ApiUtil.Assert($"Tried to set bit {i} for map {mapId}, but each map only contains {BitsPerMap}");
            return;
        }

        int index = mapId.Id * BitsPerMap + i;
        if (index > Count)
        {
            ApiUtil.Assert($"Tried to set bit {i} for map {mapId} (num {mapId.Id}), but there are only enough bits allocated for {Count/BitsPerMap} maps");
            return;
        }

        SetFlag(index, value);
    }

    public void SetFlag(int i, bool value)
    {
        if (i < 0 || i >= Count)
        {
            ApiUtil.Assert($"Tried to set out of range flag {i} (max allowed is {Count - 1})");
            return;
        }

        _set[i] = value;
    }

    public byte[] GetPacked()
    {
        // TODO: Check if .CopyTo will work
        var packed = new byte[PackedSize];
        for (int i = 0; i < Count; i++)
            packed[i >> 3] |= (byte)((_set[i] ? 1 : 0) << (i & 7));

        return packed;
    }

    public void SetPacked(byte[] packed)
    {
        ArgumentNullException.ThrowIfNull(packed);
        if (packed.Length != PackedSize)
            throw new ArgumentException($"Expected {PackedSize} bytes, but given {packed.Length}");

        for (int i = 0; i < Count; i++)
        {
            bool value = (packed[i >> 3] & (1 << (i & 7))) != 0;
            _set[i] = value;
        }
    }

    public void Serdes(string name, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (s.IsReading())
            SetPacked(s.Bytes(name, null, PackedSize));
        else
            s.Bytes(name, GetPacked(), PackedSize);

        if (s.IsCommenting())
            Describe(s);
    }

    void Describe(ISerializer s)
    {
        var sb = new StringBuilder();
        int lastMap = -1;
        int printCount = 0;
        for (int i = 0; i < Count; i++)
        {
            if (!_set[i])
                continue;

            if (BitsPerMap > 0)
            {
                var map = i / BitsPerMap;
                var offset = i % BitsPerMap;
                if (map != lastMap)
                {
                    s.Comment(sb.ToString());
                    sb.Clear();
                    sb.Append($"{new MapId(map)} ({map}): ");
                    lastMap = map;
                }

                sb.Append(offset);
                sb.Append(' ');
            }
            else
            {
                sb.Append(i);
                sb.Append(' ');
                printCount++;
                if (printCount > 16)
                {
                    s.Comment(sb.ToString());
                    sb.Clear();
                    printCount = 0;
                }
            }
        }
        s.Comment(sb.ToString());
    }
}
