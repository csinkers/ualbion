using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.WordList)]
    public class WordListLoader : IAssetLoader
    {
        const int WordLength = 21;
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));
            int wordListId = config.Id;
            var wordCount = s.BytesRemaining / WordLength;
            var words = new Dictionary<int, string>();
            for (int i = 0; i < wordCount; i++)
                words[wordListId * 100 + i] = s.FixedLengthString("Word", null, WordLength);
            return words;
        }
    }
}
