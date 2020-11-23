using System;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    public class WordListLoader : IAssetLoader
    {
        const int WordLength = 21;
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));
            s.Seek(WordLength * config.SubAssetId);
            return s.FixedLengthString(null, null, WordLength);
        }
    }
}
