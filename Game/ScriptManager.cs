using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class ScriptManager : Component
    {
        readonly MapDataId _mapId;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<ScriptManager, DoScriptEvent>((x,e) => x.Run(e))
        );

        void Run(DoScriptEvent doScriptEvent)
        {
            var assets = Resolve<IAssetManager>();
            var events = assets.LoadScript(doScriptEvent.ScriptId);
            var chain = new EventChain(0, TextSource.Map(_mapId));
            int i = 0;
            foreach(var e in events)
                chain.Events.Add(new EventNode(i++, e));

            var trigger = new TriggerChainEvent(chain, chain.FirstEvent, new EventSource.None());
            doScriptEvent.Acknowledged = true;
            trigger.OnComplete += (sender, args) => doScriptEvent.Complete();
            Raise(trigger);
        }

        public ScriptManager(MapDataId mapId) : base(Handlers) => _mapId = mapId;
    }
}
