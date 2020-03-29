using System;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events
{
    public class TriggerChainEvent : IEvent
    {
        public TriggerChainEvent(EventChain chain, IEventNode node, TriggerType trigger)
        {
            Chain = chain ?? throw new ArgumentNullException(nameof(chain));
            Node = node ?? throw new ArgumentNullException(nameof(node));
            Trigger = trigger;
        }

        public TriggerChainEvent(EventChain chain, IEventNode node, TriggerType trigger, int x, int y)
            : this(chain, node, trigger)
        {
            X = x;
            Y = y;
        }

        public TriggerChainEvent(EventChain chain, IEventNode node, TriggerType trigger, NpcCharacterId npcId)
            : this(chain, node, trigger)
        {
            NpcId = npcId;
        }

        public override string ToString() => 
            $"Triggering chain {Chain.Id} due to {Trigger} at ({X}, {Y}) (event {Node.Id}, first event {Chain.Events[0].Id})"
            + (NpcId == null ? "" : $" (Npc {NpcId})");

        public EventChain Chain { get; }
        public IEventNode Node { get; }
        public TriggerType Trigger { get; }
        public int X { get; }
        public int Y { get; }
        public NpcCharacterId? NpcId { get; }
    }
}
