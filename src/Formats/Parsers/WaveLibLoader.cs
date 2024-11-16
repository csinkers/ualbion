using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class WaveLibLoader : IAssetLoader<WaveLib>
{
    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes((WaveLib)existing, s, context);
    public WaveLib Serdes(WaveLib existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(s);
        return WaveLib.Serdes(existing, s);
    }
}
