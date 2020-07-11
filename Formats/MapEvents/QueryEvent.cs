using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class QueryEvent : MapEvent, IQueryEvent
    {
        public static IQueryEvent Serdes(IQueryEvent genericEvent, ISerializer s, AssetType textType, ushort textSourceId)
        {
            var subType = s.EnumU8("SubType", genericEvent?.QueryType ?? QueryType.IsScriptDebugModeActive);
            switch (subType)
            {
                case QueryType.InventoryHasItem:
                case QueryType.UsedItemId:
                    return QueryItemEvent.Serdes((QueryItemEvent)genericEvent, s, subType);

                case QueryType.ChosenVerb:
                    return QueryVerbEvent.Serdes((QueryVerbEvent)genericEvent, s);

                case QueryType.IsPartyMemberConscious:
                case QueryType.IsPartyMemberLeader:
                case QueryType.HasPartyMember:
                    return QueryPartyEvent.Serdes((QueryPartyEvent)genericEvent, s, subType);

                case QueryType.PreviousActionResult: break;
                case QueryType.Ticker: break;
                case QueryType.CurrentMapId: break;
                case QueryType.TriggerType: break;

                case QueryType.PromptPlayer:
                    return PromptPlayerEvent.Serdes((PromptPlayerEvent)genericEvent, s, textType, textSourceId);

                case QueryType.PromptPlayerNumeric:
                    return PromptPlayerNumericEvent.Serdes((PromptPlayerNumericEvent) genericEvent, s);

            }

            var e = (QueryEvent)genericEvent ?? new QueryEvent { QueryType = subType };
            s.Begin();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Argument = s.UInt16(nameof(Argument), e.Argument);

            ApiUtil.Assert(e.Unk4 == 0);
            ApiUtil.Assert(e.Unk5 == 0);

            s.End();
            return e;
        }

        public QueryType QueryType { get; private set; }
        public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Immediate { get; private set; } // immediate value?
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }

        public ushort Argument { get; private set; }

        public override string ToString() => $"query {QueryType} {Argument} ({Operation} {Immediate})";
        public override MapEventType EventType => MapEventType.Query;
    }
}
