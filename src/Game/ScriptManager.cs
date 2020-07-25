using System;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Map;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class ScriptManager : Component
    {
        public ScriptManager()
        {
            OnAsync<DoScriptEvent>(Run);
            On<DumpScriptEvent>(Dump);
        }

        bool Run(DoScriptEvent doScriptEvent, Action continuation)
        {
            var assets = Resolve<IAssetManager>();
            var mapManager = Resolve<IMapManager>();

            var events = assets.LoadScript(doScriptEvent.ScriptId);
            var nodes = new EventNode[events.Count];
            var chain = new EventChain(0);

            // Create, link and add all the nodes.
            for (ushort i = 0; i < events.Count;     i++) nodes[i] = new EventNode(i, events[i]);
            for (ushort i = 0; i < events.Count - 1; i++) nodes[i].Next = nodes[i + 1];
            for (ushort i = 0; i < events.Count;     i++) chain.Events.Add(nodes[i]);

            var source = new EventSource.Map(mapManager.Current.MapId, TriggerType.Default, 0, 0); // TODO: Is there a better trigger type for this?
            var trigger = new TriggerChainEvent(chain, chain.FirstEvent, source);
            return RaiseAsync(trigger, continuation) > 0;
        }

        void Dump(DumpScriptEvent dumpScriptEvent)
        {
            var assets = Resolve<IAssetManager>();
            var events = assets.LoadScript(dumpScriptEvent.ScriptId);
            foreach (var e in events)
                Raise(new LogEvent(LogEvent.Level.Info, e.ToString()));
        }
    }

    [Event("dump_script")]
    public class DumpScriptEvent : GameEvent
    {
        public DumpScriptEvent(ScriptId scriptId) => ScriptId = scriptId;
        [EventPart("id")] public ScriptId ScriptId { get; }
    }
}
