using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class JsonStringLoader : IAssetLoader
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var bytes = s.ByteArray(null, null, (int)s.BytesRemaining);
            var text = Encoding.UTF8.GetString(bytes);
            return 
                JsonConvert.DeserializeObject<IDictionary<string, string>>(text)
                    .ToDictionary(x => TextId.Parse(x.Key), x => x.Value);
        }
    }
}