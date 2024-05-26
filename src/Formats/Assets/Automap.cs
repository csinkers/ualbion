using System;
using System.Collections;
using SerdesNet;

namespace UAlbion.Formats.Assets;

public class Automap
{
    BitArray _discovered;
    public int Width { get; set; }
    public int Height => Width == 0 ? 0 : _discovered.Length / Width;

    public byte[] AsBytes
    {
        get
        {
            var length = (_discovered.Length + 7) / 8;
            var bytes = new byte[length];
            for (int i = 0; i < _discovered.Length; i++)
                bytes[i >> 3] |= (byte)(_discovered[i] ? 1 << (i & 7) : 0);
            return bytes;
        }
        set
        {
            if (value == null)
            {
                _discovered = null;
                return;
            }

            _discovered = new BitArray(value.Length * 8);
            for (int i = 0; i < _discovered.Length; i++)
                _discovered[i] = (value[i >> 3] & (1 << (i & 7))) != 0;
        }
    }

    public bool this[int index] => _discovered[index];
    public bool this[int x, int y] => _discovered[y * Width + x];

    Automap() { }
    public Automap(int width, int height)
    {
        Width = width;
        _discovered = new BitArray(width * height);
    }

    public static Automap Serdes(Automap map, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        int length;
        byte[] bytes = null;
        if (s.IsReading())
        {
            map ??= new Automap();
            length = (int)s.BytesRemaining;
        }
        else
        {
            ArgumentNullException.ThrowIfNull(map);
            bytes = map.AsBytes;
            length = bytes.Length;
        }

        bytes = s.Bytes(null, bytes, length);

        if (s.IsReading())
            map.AsBytes = bytes;

        return map;
    }
}