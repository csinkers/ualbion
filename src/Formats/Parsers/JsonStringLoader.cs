using System;
using System.Text;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class JsonStringLoader : IAssetLoader
    {
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
            var text = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<IntStringDictionary>(text);
        }
    }
}
