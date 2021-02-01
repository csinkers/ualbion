using System;
using System.Linq;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class AlbionStringTableLoader : IAssetLoader<AlbionStringCollection>
    {
        public AlbionStringCollection Serdes(AlbionStringCollection existing, AssetInfo config, AssetMapping mapping, ISerializer s)
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
                return new AlbionStringCollection(strings);
            }
            else
            {
                var stringCount = s.UInt16("StringCount", 0);
                var byteArrays = existing.Select(FormatUtil.BytesFrom850String).ToArray();
                for (int i = 0; i < stringCount; i++)
                    s.UInt16(null, (ushort)byteArrays[i].Length);

                for (int i = 0; i < stringCount; i++)
                    s.ByteArray(null, byteArrays[i], byteArrays[i].Length);
                return existing;
            }
        }

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes(existing as AlbionStringCollection, config, mapping, s);
    }
}
