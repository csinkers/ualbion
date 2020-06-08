using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.WordList)]
    public class WordListLoader : IAssetLoader
    {
        const int WordLength = 21;
        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            int wordListId = config.Id;
            var wordCount = streamLength / WordLength;
            var s = new GenericBinaryReader(br, streamLength, FormatUtil.BytesTo850String, ApiUtil.Assert);
            var words = new Dictionary<int, string>();
            for (int i = 0; i < wordCount; i++)
                words[wordListId * 100 + i] = s.FixedLengthString("Word", null, WordLength);
            return words;
        }
    }
}
