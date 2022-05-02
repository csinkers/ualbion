using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class EventSetLoader : IAssetLoader<EventSet>
{
    public EventSet Serdes(EventSet existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        return EventSet.Serdes(info.AssetId, existing, context.Mapping, s);
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes(existing as EventSet, info, s, context);
}
