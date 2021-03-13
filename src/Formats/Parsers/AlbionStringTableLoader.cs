using System;
using System.Linq;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class AlbionStringTableLoader : IAssetLoader<ListStringCollection>
    {
        public ListStringCollection Serdes(ListStringCollection existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (s.IsReading())
            {
                var stringCount = s.UInt16("StringCount", 0);
                var stringLengths = new int[stringCount];
                for (int i = 0; i < stringCount; i++)
                    stringLengths[i] = s.UInt16(null, 0);

                var strings = new string[stringCount];
                for (int i = 0; i < stringCount; i++)
                    strings[i] = s.FixedLengthString(null, null, stringLengths[i]);
                return new ListStringCollection(strings);
            }
            else
            {
                if (existing == null) throw new ArgumentNullException(nameof(existing));
                var stringCount = s.UInt16("StringCount", (ushort)existing.Count);
                var byteArrays = existing.Select(FormatUtil.BytesFrom850String).ToArray();
                for (int i = 0; i < stringCount; i++)
                    s.UInt16(null, (ushort)(byteArrays[i].Length + 1));

                for (int i = 0; i < stringCount; i++)
                {
                    s.Bytes(null, byteArrays[i], byteArrays[i].Length);
                    s.UInt8(null, 0);
                }

                return existing;
            }
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes(existing as ListStringCollection, info, mapping, s);
    }
}
