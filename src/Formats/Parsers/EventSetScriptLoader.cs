using System;
using System.Text;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Parsers;

public class EventSetScriptLoader : Component, IAssetLoader<EventSet>
{
    public EventSet Serdes(EventSet existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var id = (EventSetId)context.AssetId;
        if (s.IsWriting())
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            var assets = Resolve<IAssetManager>();
            var script = Decompile(id, existing, assets);
            var bytes = Encoding.UTF8.GetBytes(script);
            s.Bytes(null, bytes, bytes.Length);
        }
        else
        {
            var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
            var script = Encoding.UTF8.GetString(bytes);
            var eventLayout = AlbionCompiler.Compile(script);
            return new EventSet(context.AssetId, eventLayout.Events, eventLayout.Chains);
        }

        return existing;
    }

    static string Decompile(EventSetId id, EventSet set, IAssetManager assets)
    {
        var eventFormatter = new EventFormatter(assets.LoadString, id.ToEventText());
        return eventFormatter.Decompile(set.Events, set.Chains, null).Script;
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((EventSet)existing, s, context);
}