using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("event_used")]
public class QueryEventUsedEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.EventUsed;
    public static QueryEventUsedEvent Serdes(QueryEventUsedEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new QueryEventUsedEvent();
        int zeroes = s.UInt8("zero2", 0);
        zeroes += s.UInt8("zero3", 0);
        zeroes += s.UInt8("zero4", 0);
        zeroes += s.UInt8("zero5", 0);
        zeroes += s.UInt16("zero6",0);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "QueryEventUsedEvent: Expected all fields to be 0");
        return e;
    }
}