using System;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Map;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events
{
    public class TriggerChainEvent : IEvent
    {
        public TriggerChainEvent(EventChain chain, IEventNode node, EventSource source)
        {
            Chain = chain ?? throw new ArgumentNullException(nameof(chain));
            Node = node ?? throw new ArgumentNullException(nameof(node));
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public TriggerChainEvent(EventChain chain, IEventNode node, TriggerType trigger, int x, int y) 
            : this(chain, node, new EventSource.Map(trigger, x, y)) { }

        public TriggerChainEvent(EventChain chain, IEventNode node, NpcCharacterId npcId)
            : this(chain, node, new EventSource.Npc(npcId)) { }

        public override string ToString() => 
            $"Triggering chain {Chain.Id} due to {Source} (event {Node.Id}, first event {Chain.Events[0].Id})";

        public EventChain Chain { get; }
        public IEventNode Node { get; }
        public EventSource Source { get; }
        public void Complete() => OnComplete?.Invoke(this, EventArgs.Empty);
        public event EventHandler<EventArgs> OnComplete;
    }
}
