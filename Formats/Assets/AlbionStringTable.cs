using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

            Debug.Assert(br.BaseStream.Position == startOffset + streamLength);
            return strings;
        }
    }
}