using System;
using System.Linq;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class AlbionStringTableLoader : IAssetLoader<ListStringSet>
{
    public ListStringSet Serdes(ListStringSet existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (s.IsReading())
        {
            var stringCount = s.UInt16("StringCount", 0);
            var stringLengths = new int[stringCount];
            for (int i = 0; i < stringCount; i++)
                stringLengths[i] = s.UInt16(null, 0);

            var strings = new string[stringCount];
            for (int i = 0; i < stringCount; i++)
                strings[i] = s.AlbionString(null, null, stringLengths[i]);
            return new ListStringSet(strings);
        }
        else
        {
            ArgumentNullException.ThrowIfNull(existing);
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

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes(existing as ListStringSet, s, context);
}
