using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class WaveLibLoader : IAssetLoader<WaveLib>
{
    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes((WaveLib)existing, info, mapping, s, jsonUtil);
    public WaveLib Serdes(WaveLib existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (info == null) throw new ArgumentNullException(nameof(info));
        return WaveLib.Serdes(existing, s);
    }
}