using System;
using UAlbion.Api;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events
{
    public class TriggerChainEvent : IEvent
    {
        public TriggerChainEvent(IEventNode chain, TriggerType trigger)
        {
            Chain = chain ?? throw new ArgumentNullException(nameof(chain));
            Trigger = trigger;
        }

        public IEventNode Chain { get; }
        public TriggerType Trigger { get; }
    }
}