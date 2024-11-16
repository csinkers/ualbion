using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Parsers;

public class MapLoader : IAssetLoader<IMapData>
{
    public IMapData Serdes(IMapData existing, ISerdes s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return BaseMapData.Serdes(context.AssetId, existing, context.Mapping, s);
    }

    public object Serdes(object existing, ISerdes s, AssetLoadContext context)
        => Serdes(existing as IMapData, s, context);
}