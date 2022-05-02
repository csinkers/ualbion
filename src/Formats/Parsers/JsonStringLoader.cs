using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class JsonStringLoader : IAssetLoader
{
    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));
        var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
        return context.Json.Deserialize<IntStringDictionary>(bytes);
    }
}
