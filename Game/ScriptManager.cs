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
            On<DoScriptEvent>(Run);
            On<DumpScriptEvent>(Dump);
        }

        void Run(DoScriptEvent doScriptEvent)
        {
            var assets = Resolve<IAssetManager>();
            var mapManager = Resolve<IMapManager>();

            var events = assets.LoadScript(doScriptEvent.ScriptId);
            var chain = new EventChain(0);
            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];
                chain.Events.Add(new EventNode(i, e)
                {
                    NextEventId = i + 1 == events.Count ? null : (ushort?)(i + 1)
                });
            }

            for (int i = 0; i < chain.Events.Count - 1; i++)
                chain.Events[i].NextEvent = chain.Events[i + 1];

            var source = new EventSource.Map(mapManager.Current.MapId, TriggerType.Default, 0, 0); // TODO: Is there a better trigger type for this?

            var trigger = new TriggerChainEvent(chain, chain.FirstEvent, source);
            doScriptEvent.Acknowledge();
            trigger.OnComplete += (sender, args) => doScriptEvent.Complete();
            Raise(trigger);
        }

        void Dump(DumpScriptEvent dumpScriptEvent)
        {
            var assets = Resolve<IAssetManager>();
            var events = assets.LoadScript(dumpScriptEvent.ScriptId);
            foreach (var e in events)
                Raise(new LogEvent(LogEvent.Level.Info, e.ToString()));
        }
    }

    [Api.Event("dump_script")]
    public class DumpScriptEvent : GameEvent
    {
        public DumpScriptEvent(ScriptId scriptId) => ScriptId = scriptId;
        [EventPart("id")] public ScriptId ScriptId { get; }
    }
}
