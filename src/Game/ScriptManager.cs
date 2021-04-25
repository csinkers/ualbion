using System;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
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
            if (events == null)
            {
                Error($"Could not load script {doScriptEvent.ScriptId}");
                return false;
            }

            var nodes = new EventNode[events.Count];

            // Create, link and add all the nodes.
            for (ushort i = 0; i < events.Count;     i++) nodes[i] = new EventNode(i, events[i]);
            for (ushort i = 0; i < events.Count - 1; i++) nodes[i].Next = nodes[i + 1];

            var source = new EventSource(mapManager.Current.MapId, mapManager.Current.MapId.ToMapText(), TriggerTypes.Default); // TODO: Is there a better trigger type for this?
            var trigger = new TriggerChainEvent(AssetId.None, 0, nodes[0], source);
            return RaiseAsync(trigger, continuation) > 0;
        }

        void Dump(DumpScriptEvent dumpScriptEvent)
        {
            var assets = Resolve<IAssetManager>();
            var events = assets.LoadScript(dumpScriptEvent.ScriptId);
            foreach (var e in events)
                Info(e.ToString());
        }
    }

    [Event("dump_script")]
    public class DumpScriptEvent : GameEvent
    {
        public DumpScriptEvent(ScriptId scriptId) => ScriptId = scriptId;
        [EventPart("id")] public ScriptId ScriptId { get; }
    }
}
