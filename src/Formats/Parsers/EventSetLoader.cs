using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers;

public class EventSetLoader : IAssetLoader<EventSet>
{
    public EventSet Serdes(EventSet existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        return EventSet.Serdes(info.AssetId, existing, mapping, s);
    }

    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes(existing as EventSet, info, mapping, s, jsonUtil);
}