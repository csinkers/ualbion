using System.Collections.Generic;
using System.IO;
using UAlbion.Api;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Assets
{
    [AssetLoader(FileFormat.StringTable)]
    public class AlbionStringTableLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            IDictionary<int, string> strings = new Dictionary<int, string>();
            var startOffset = br.BaseStream.Position;
            var stringCount = br.ReadUInt16();
            var stringLengths = new int[stringCount];

            for (int i = 0; i < stringCount; i++)
                stringLengths[i] = br.ReadUInt16();

            for (int i = 0; i < stringCount; i++)
            {
                var bytes = br.ReadBytes(stringLengths[i]);
                strings[i] = FormatUtil.BytesTo850String(bytes);
            }

            ApiUtil.Assert(br.BaseStream.Position == startOffset + streamLength);
            return strings;
        }

        // public StringTable Serdes(StringTable existing, ISerializer s, string name, AssetInfo config) => StringTable.Serdes(existing, s);
    }
    /*
    public class StringTable
    {
        readonly string[] _strings;
        StringTable(string[] strings) => _strings = strings;
        public string this[int key] => _strings[key];

        public static StringTable Serdes(StringTable existing, ISerializer s)
        {
            var strings = existing?._strings;
            var stringCount = s.UInt16("StringCount", (ushort)(strings?.Length ?? 0));
            strings ??= new string[stringCount];

            var stringLengths = strings.Select(x => (ushort)(x?.Length ?? 0)).ToArray();
            for (int i = 0; i < stringCount; i++)
                stringLengths[i] = s.UInt16(null, stringLengths[i]);

            for (int i = 0; i < stringCount; i++)
                strings[i] = s.FixedLengthString(null, strings[i], stringLengths[i]);

            s.Check();
            return existing ?? new StringTable(strings);
        }
    }
    */
}
