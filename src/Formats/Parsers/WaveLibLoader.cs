using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class WaveLibLoader : IAssetLoader<WaveLib>
{
    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((WaveLib)existing, s, context);
    public WaveLib Serdes(WaveLib existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        return WaveLib.Serdes(existing, s);
    }
}
