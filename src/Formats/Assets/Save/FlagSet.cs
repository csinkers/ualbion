using System;
using System.Collections;
using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Save;

public class FlagSet
{
    readonly BitArray _set;

    public int Offset { get; }
    public int BitsPerMap { get; }
    public int Count => _set.Count;
    public int PackedSize => (Count + 7) / 8;

    public FlagSet(int offset, int count, int bitsPerMap = 0)
    {
        Offset = offset;
        BitsPerMap = bitsPerMap;
        _set = new BitArray(count);
    }

    public bool GetFlag(int i) => i >= Offset && i < Count + Offset && _set[i - Offset];
    public void SetFlag(int i, bool value)
    {
        if (i < Offset || i >= Count + Offset)
        {
            ApiUtil.Assert($"Tried to set out of range flag {i}");
            return;
        }

        _set[i - Offset] = value;
    }

    public byte[] GetPacked()
    {
        // TODO: Check if .CopyTo will work
        var packed = new byte[PackedSize];
        for (int i = 0; i < Count; i++)
            packed[i / 8] |= (byte)((_set[i] ? 1 : 0) << (i % 8));

        return packed;
    }

    public void SetPacked(byte[] packed)
    {
        if (packed == null) throw new ArgumentNullException(nameof(packed));
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
            if (BitsPerMap > 0)
            {
                var map = i / BitsPerMap;
                var offset = i % BitsPerMap;
                if (map != lastMap)
                {
                    sb.AppendLine();
                    sb.Append($"{new MapId(AssetType.Map, map)} ({map}): ");
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
                    sb.AppendLine();
                    printCount = 0;
                }
            }
        }
        s.Comment(sb.ToString());
    }
}