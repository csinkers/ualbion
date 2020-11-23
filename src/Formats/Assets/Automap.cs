using System;
using SerdesNet;

namespace UAlbion.Formats.Assets
{
    public class Automap
    {
        byte[] _bytes;
        public int Width { get; set; }
        public int Height => Width == 0 ? 0 : _bytes.Length / Width;
        public static Automap Serdes(Automap map, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            map ??= new Automap();
            var length = (int)(s.Mode == SerializerMode.Reading ? s.BytesRemaining : map._bytes.Length);
            map._bytes = s.ByteArray(null, map._bytes, length);
            return map;
        }
    }
}