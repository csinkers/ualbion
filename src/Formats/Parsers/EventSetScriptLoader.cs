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
    public EventSet Serdes(EventSet existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var id = (EventSetId)info.AssetId;
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
            var eventSetId = (EventSetId)info.AssetId;
            var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
            var script = Encoding.UTF8.GetString(bytes);
            var eventLayout = AlbionCompiler.Compile(script, eventSetId.ToEventText());
            return new EventSet(info.AssetId, eventLayout.Events, eventLayout.Chains);
        }

        return existing;
    }

    static string Decompile(EventSetId id, EventSet set, IAssetManager assets)
    {
        var sb = new StringBuilder();
        var eventFormatter = new EventFormatter(assets.LoadString, id.ToEventText());
        eventFormatter.FormatEventSetDecompiled(sb, set.Events, set.Chains, null, 0);
        return sb.ToString();
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes((EventSet)existing, info, s, context);
}