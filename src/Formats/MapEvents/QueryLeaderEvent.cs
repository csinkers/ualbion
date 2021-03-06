﻿using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("query_leader")]
    public class QueryLeaderEvent : QueryEvent
    {
        public override QueryType QueryType => QueryType.Leader;
        [EventPart("op")] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        [EventPart("imm")] public byte Immediate { get; private set; } // immediate value?
        [EventPart("party_member")] public PartyMemberId PartyMemberId { get; private set; } // => AssetType == AssetType.PartyMember
        QueryLeaderEvent() { }
        public QueryLeaderEvent(QueryOperation operation, byte immediate, PartyMemberId partyMemberId)
        {
            Operation = operation;
            Immediate = immediate;
            PartyMemberId = partyMemberId;
        }
        public static QueryLeaderEvent Serdes(QueryLeaderEvent e, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new QueryLeaderEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            int zeroes = s.UInt8(null, 0);
            zeroes += s.UInt8(null, 0);
            e.PartyMemberId = PartyMemberId.SerdesU16(nameof(PartyMemberId), e.PartyMemberId, mapping, s);
            // field 8 is the next event id when the condition is and is deserialised as part of the BranchEventNode that this event should belong to.

            s.Assert(zeroes == 0, "QueryLeaderEvent: Expected fields 3,4 to be 0");
            return e;
        }
    }
}