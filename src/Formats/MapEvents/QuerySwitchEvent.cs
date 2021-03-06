﻿using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("query_switch")]
    public class QuerySwitchEvent : QueryEvent
    {
        public override QueryType QueryType => QueryType.Switch;
        [EventPart("op")] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        [EventPart("imm")] public byte Immediate { get; private set; } // immediate value?
        [EventPart("switch")] public SwitchId SwitchId { get; private set; } // => AssetType == AssetType.Switch
        QuerySwitchEvent() { }
        public QuerySwitchEvent(QueryOperation operation, byte immediate, SwitchId switchId)
        {
            Operation = operation;
            Immediate = immediate;
            SwitchId = switchId;
        }
        public static QuerySwitchEvent Serdes(QuerySwitchEvent e, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new QuerySwitchEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            int zeroes = s.UInt8(null, 0);
            zeroes += s.UInt8(null, 0);
            e.SwitchId = SwitchId.SerdesU16(nameof(SwitchId), e.SwitchId, mapping, s);
            // field 8 is the next event id when the condition is and is deserialised as part of the BranchEventNode that this event should belong to.
            s.Assert(zeroes == 0, "QuerySwitchEvent: Expected fields 3,4 to be 0");
            return e;
        }
    }
}