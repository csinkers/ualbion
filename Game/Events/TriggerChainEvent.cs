using System;
using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    public class TriggerChainEvent : IEvent
    {
        public TriggerChainEvent(EventChain chain, TriggerType trigger, int x, int y)
        {
            Chain = chain ?? throw new ArgumentNullException(nameof(chain));
            Trigger = trigger;
            X = x;
            Y = y;
        }

        public override string ToString() => $"Triggering chain {Chain.Id} due to {Trigger} (first event {Chain.Events[0].Id})";

        public EventChain Chain { get; }
        public TriggerType Trigger { get; }
        public int X { get; }
        public int Y { get; }
    }
}