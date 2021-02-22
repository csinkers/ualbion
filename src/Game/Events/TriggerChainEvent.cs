using System;
using UAlbion.Api;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events
{
    public class TriggerChainEvent : IAsyncEvent
    {
        public TriggerChainEvent(EventChain chain, IEventNode node, EventSource source)
        {
            Chain = chain ?? throw new ArgumentNullException(nameof(chain));
            Node = node ?? throw new ArgumentNullException(nameof(node));
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public override string ToString() =>
            $"Triggering chain {Chain.Id} due to {Source} (event {Node.Id}, first event {Chain.Events[0].Id})";

        public EventChain Chain { get; }
        public IEventNode Node { get; }
        public EventSource Source { get; }
    }
}
