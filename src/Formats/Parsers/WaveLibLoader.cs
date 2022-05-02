using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class WaveLibLoader : IAssetLoader<WaveLib>
{
    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes((WaveLib)existing, info, s, context);
    public WaveLib Serdes(WaveLib existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (info == null) throw new ArgumentNullException(nameof(info));
        return WaveLib.Serdes(existing, s);
    }
}
