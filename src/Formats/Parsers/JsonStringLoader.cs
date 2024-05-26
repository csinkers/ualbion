using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class JsonStringLoader : IAssetLoader
{
    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(context);
        var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
        return context.Json.Deserialize<IntStringDictionary>(bytes);
    }
}