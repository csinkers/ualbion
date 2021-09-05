using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class JsonStringLoader : IAssetLoader
    {
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
            var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
            return jsonUtil.Deserialize<IntStringDictionary>(bytes);
        }
    }
}
