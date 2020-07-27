using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic
{
    public class FullByteOrientedRleChunk : FlicChunk
    {
        public byte[] PixelData { get; private set; }

        public override FlicChunkType Type => FlicChunkType.FullByteOrientedRle;

        IEnumerable<byte> Decompress(IEnumerable<byte> compressed)
        {
            var e = compressed.GetEnumerator();
            while (e.MoveNext())
            {
                sbyte type = (sbyte)e.Current;
                if (type >= 0)
                {
                    if (!e.MoveNext())
                        yield break;

                    byte value = e.Current;
                    while (type != 0)
                    {
                        yield return value;
                        type--;
                    }
                }
                else
                {
                    while(type != 0 && e.MoveNext())
                    {
                        yield return e.Current;
                        type++;
                    }
                }
            }
        }

        protected override uint SerdesBody(uint length, ISerializer s)
        {
            if (s.Mode != SerializerMode.Reading)
                throw new NotImplementedException();

            var compressed = s.ByteArray(nameof(PixelData), null, (int)length);
            PixelData = Decompress(compressed).ToArray();
            return (uint)compressed.Length;
        }
    }
}
