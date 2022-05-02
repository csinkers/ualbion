using UAlbion.Api.Eventing;

namespace UAlbion.Formats.ScriptEvents;

[Event("do_event_chain")] // USED IN SCRIPT
public class DoEventChainEvent : Event
{
    public DoEventChainEvent(int eventChainId) { EventChainId = eventChainId; }
    [EventPart("eventChainId")] public int EventChainId { get; }
}