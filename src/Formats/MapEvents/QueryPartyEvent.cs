using System;
using SerdesNet;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class QueryPartyEvent : MapEvent, IQueryEvent
    {
        public static QueryPartyEvent Serdes(QueryPartyEvent e, ISerializer s, QueryType subType)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new QueryPartyEvent();
            s.Begin();
            e.QueryType = subType;
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.PartyMemberId = s.TransformEnumU8(nameof(PartyMemberId), e.PartyMemberId, StoreIncrementedNullZero<PartyCharacterId>.Instance);
            s.UInt8("pad", 0);
            s.End();
            return e;
        }

        public PartyCharacterId? PartyMemberId { get; private set; }
        public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Immediate { get; private set; } // immediate value?
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }

        public override string ToString() => $"query {QueryType} {PartyMemberId} ({Operation} {Immediate})";
        public override MapEventType EventType => MapEventType.Query;
        public QueryType QueryType { get; private set; }
    }
}
