using System;
using System.Collections;
using SerdesNet;

namespace UAlbion.Formats.Assets
{
    public class Automap
    {
        BitArray _discovered;
        public int Width { get; set; }
        public int Height => Width == 0 ? 0 : _discovered.Length / Width;
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
            if (s == null) throw new ArgumentNullException(nameof(s));
            map ??= new Automap();
            var length = (int)(s.IsReading() ? s.BytesRemaining : (map._discovered.Length + 7) / 8);
            var bytes = new byte[length];

            if (map._discovered != null)
                for (int i = 0; i < map._discovered.Length; i++)
                    bytes[i >> 3] |= (byte)(map._discovered[i] ? 0 : 1 << (i & 7));

            bytes = s.ByteArray(null, bytes, length);

            map._discovered ??= new BitArray(length * 8);
            for (int i = 0; i < map._discovered.Length; i++)
                map._discovered[i] = (bytes[i >> 3] & (1 << (i & 7))) != 0;

            return map;
        }
    }
}