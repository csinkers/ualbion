using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.MapEvents
{
    public class QueryEvent : MapEvent, IQueryEvent
    {
        public static IQueryEvent Serdes(IQueryEvent genericEvent, ISerializer s, AssetType textType, ushort textSourceId)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
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

        public override string ToString() => QueryType switch
        {
            QueryType.Ticker => $"query {QueryType} {(TickerId)Argument} ({Operation} {Immediate})",
            QueryType.TemporarySwitch => $"query {QueryType} {(SwitchId)Argument} ({Operation} {Immediate})",
            _ => $"query {QueryType} {Argument} ({Operation} {Immediate})"
        };

        public override MapEventType EventType => MapEventType.Query;

        public static QueryEvent TemporarySwitch(SwitchId id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.TemporarySwitch,
            Argument = (ushort)id,
            Operation = operation,
            Immediate = immediate
        };

        public static QueryEvent HasPartyMember(PartyCharacterId id) => new QueryEvent { QueryType = QueryType.HasPartyMember, Argument = (ushort)id };
        public static QueryEvent InventoryHasItem(ItemId id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.InventoryHasItem,
            Argument = (ushort)id,
            Operation = operation,
            Immediate = immediate
        };

        public static QueryEvent UsedItemId(ItemId id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.UsedItemId,
            Argument = (ushort)id,
            Operation = operation,
            Immediate = immediate
        };
        public static QueryEvent PreviousActionResult(QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.PreviousActionResult,
            Operation = operation,
            Immediate = immediate
        };
        public static QueryEvent IsScriptDebugModeActive() => new QueryEvent { QueryType = QueryType.IsScriptDebugModeActive, };
        public static QueryEvent IsNpcActive(ushort id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.IsNpcActive,
            Argument = id,
            Operation = operation,
            Immediate = immediate
        };
        public static QueryEvent HasEnoughGold(ushort amount) => new QueryEvent { QueryType = QueryType.HasEnoughGold, Argument = amount };
        public static QueryEvent RandomChance(ushort percentage) => new QueryEvent { QueryType = QueryType.RandomChance, Argument = percentage, };
        public static QueryEvent IsPartyMemberConscious(PartyCharacterId id) => new QueryEvent { QueryType = QueryType.IsPartyMemberConscious, Argument = (ushort)id, };
        public static QueryEvent IsPartyMemberLeader(PartyCharacterId id) => new QueryEvent { QueryType = QueryType.IsPartyMemberLeader, Argument = (ushort)id, };
        public static QueryEvent Ticker(TickerId id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.Ticker,
            Argument = (ushort)id,
            Operation = operation,
            Immediate = immediate
        };
        public static QueryEvent CurrentMapId(MapDataId id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.CurrentMapId,
            Argument = (ushort)id,
            Operation = operation,
            Immediate = immediate
        };
        public static QueryEvent PromptPlayer(ushort textId, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.PromptPlayer,
            Argument = textId,
            Operation = operation,
            Immediate = immediate
        };
        public static QueryEvent TriggerType(TriggerTypes id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.TriggerType,
            Argument = (ushort)id,
            Operation = operation,
            Immediate = immediate
        };
        public static QueryEvent EventAlreadyUsed(ushort id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.EventAlreadyUsed,
            Argument = id,
            Operation = operation,
            Immediate = immediate
        };
        public static QueryEvent IsDemoVersion() => new QueryEvent { QueryType = QueryType.IsDemoVersion, };
        public static QueryEvent PromptPlayerNumeric(ushort value) => new QueryEvent { QueryType = QueryType.PromptPlayerNumeric, Argument = value, };
    }
}
