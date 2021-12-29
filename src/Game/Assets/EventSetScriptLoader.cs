using System;
using System.Collections.Generic;
using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Scripting;

namespace UAlbion.Game.Assets;

public class EventSetScriptLoader : Component, IAssetLoader<EventSet>
{
    public EventSet Serdes(EventSet existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (s == null) throw new ArgumentNullException(nameof(s));

        var id = (EventSetId)info.AssetId;
        if (s.IsWriting())
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            var script = Decompile(id, existing);
            var bytes = Encoding.UTF8.GetBytes(script);
            s.Bytes(null, bytes, bytes.Length);
        }
        else
        {
            var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
            var script = Encoding.UTF8.GetString(bytes);
            var eventLayout = ScriptCompiler.Compile(script);
            return new EventSet(info.AssetId, eventLayout.Events, eventLayout.Chains);
        }

        return existing;
    }

    string Decompile(EventSetId id, EventSet set)
    {
        var sb = new StringBuilder();
        var assets = Resolve<IAssetManager>();
        var eventFormatter = new EventFormatter(assets.LoadString, id.ToEventText());
        eventFormatter.FormatEventSetDecompiled(sb, set.Events, set.Chains, null, 0);
        return sb.ToString();
    }

    public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        => Serdes((EventSet)existing, info, mapping, s, jsonUtil);
}