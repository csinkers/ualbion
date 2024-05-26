using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("get_switch")]
public class QuerySwitchEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.Switch;
    [EventPart("switch")] public SwitchId SwitchId { get; private set; } // => AssetType == AssetType.Switch
    [EventPart("op", true, QueryOperation.NonZero)] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
    [EventPart("imm", true, (byte)0)] public byte Immediate { get; private set; } // immediate value?
    QuerySwitchEvent() { }
    public QuerySwitchEvent(SwitchId switchId, QueryOperation operation, byte immediate)
    {
        Operation = operation;
        Immediate = immediate;
        SwitchId = switchId;
    }
    public static QuerySwitchEvent Serdes(QuerySwitchEvent e, AssetMapping mapping, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new QuerySwitchEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.SwitchId = SwitchId.SerdesU16(nameof(SwitchId), e.SwitchId, mapping, s);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.
        s.Assert(zeroes == 0, "QuerySwitchEvent: Expected fields 3,4 to be 0");
        return e;
    }
}