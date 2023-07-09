using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class EventSetLoader : IAssetLoader<EventSet>
{
    public EventSet Serdes(EventSet existing, ISerializer s, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        return EventSet.Serdes(context.AssetId, existing, context.Mapping, s);
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes(existing as EventSet, s, context);
}
