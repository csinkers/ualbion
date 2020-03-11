using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("do_event_chain")]
    public class DoEventChainEvent : GameEvent
    {
        public DoEventChainEvent(int eventChainId) { EventChainId = eventChainId; }
        [EventPart("eventChainId")] public int EventChainId { get; }
    }
}
