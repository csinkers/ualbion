using System;
using UAlbion.Api;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events
{
    public class TriggerChainEvent : IEvent
    {
        public TriggerChainEvent(IEventNode chain, TriggerType trigger, int x = 0, int y = 0)
        {
            Chain = chain ?? throw new ArgumentNullException(nameof(chain));
            Trigger = trigger;
            X = x;
            Y = y;
        }

        public override string ToString() => $"Triggering chain {Chain.Id} due to {Trigger}";

        public IEventNode Chain { get; }
        public TriggerType Trigger { get; }
        public int X { get; }
        public int Y { get; }
    }
}