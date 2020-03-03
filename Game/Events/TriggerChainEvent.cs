using System;
using UAlbion.Api;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events
{
    public class TriggerChainEvent : IEvent
    {
        public TriggerChainEvent(EventChain chain, IEventNode node, TriggerType trigger, int x, int y)
        {
            Chain = chain ?? throw new ArgumentNullException(nameof(chain));
            Node = node ?? throw new ArgumentNullException(nameof(node));
            Trigger = trigger;
            X = x;
            Y = y;
        }

        public override string ToString() => $"Triggering chain {Chain.Id} due to {Trigger} (event {Node.Id}, first event {Chain.Events[0].Id})";

        public EventChain Chain { get; }
        public IEventNode Node { get; }
        public TriggerType Trigger { get; }
        public int X { get; }
        public int Y { get; }
    }
}