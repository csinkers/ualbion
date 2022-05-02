using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Parsers;

public class MapLoader : IAssetLoader<IMapData>
{
    public IMapData Serdes(IMapData existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        return BaseMapData.Serdes(info, existing, context.Mapping, s);
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes(existing as IMapData, info, s, context);
}
