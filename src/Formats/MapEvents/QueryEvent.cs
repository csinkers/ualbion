using System;
using System.Globalization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.MapEvents
{
    public class QueryEvent : MapEvent, IBranchingEvent
    {
        QueryEvent() { }
        QueryEvent(TextId textSourceId) { TextSourceId = textSourceId; }
        public static QueryEvent Serdes(QueryEvent e, AssetMapping mapping, ISerializer s, TextId textSourceId)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            e ??= new QueryEvent(textSourceId);
            e.QueryType = s.EnumU8(nameof(QueryType), e.QueryType);
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);

            e._value = e.AssetType != AssetType.None && e.AssetType != AssetType.Unknown
                ? AssetId.SerdesU16("Argument", AssetId.FromUInt32(e._value), e.AssetType, mapping, s).ToUInt32()
                : s.UInt16("Argument", (ushort)e._value);

            ApiUtil.Assert(e.Unk4 == 0);
            ApiUtil.Assert(e.Unk5 == 0);

            return e;
        }

        AssetType AssetType =>
            QueryType switch
            {
                QueryType.ChosenVerb             => AssetType.None,
                QueryType.IsDemoVersion          => AssetType.None,
                QueryType.PreviousActionResult   => AssetType.None,
                QueryType.TriggerType            => AssetType.None,
                QueryType.PromptPlayerNumeric    => AssetType.None,
                QueryType.InventoryHasItem       => AssetType.Item,
                QueryType.UsedItemId             => AssetType.Item,
                QueryType.IsPartyMemberConscious => AssetType.PartyMember,
                QueryType.IsPartyMemberLeader    => AssetType.PartyMember,
                QueryType.HasPartyMember         => AssetType.PartyMember,
                QueryType.Ticker                 => AssetType.Ticker,
                QueryType.TemporarySwitch        => AssetType.Switch,
                QueryType.IsNpcActive            => AssetType.Npc,
                QueryType.CurrentMapId           => AssetType.Map,
                _ => AssetType.Unknown
            };

        public TextId TextSourceId { get; }
        public QueryType QueryType { get; private set; }
        public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Immediate { get; private set; } // immediate value?
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }

        uint _value;

        public ushort Argument => AssetType == AssetType.Unknown || AssetType == AssetType.None
            ? (ushort)_value
            : throw new InvalidOperationException($"Tried to retrieve the Argument of a {QueryType} query event");

        public ItemId ItemId => AssetType == AssetType.Item
            ? ItemId.FromUInt32(_value)
            : throw new InvalidOperationException($"Tried to retrieve the ItemId of a {QueryType} query event");

        public PartyMemberId PartyMemberId => AssetType == AssetType.PartyMember
            ? PartyMemberId.FromUInt32(_value)
            : throw new InvalidOperationException($"Tried to retrieve the PartyMemberId of a {QueryType} query event");

        public TickerId TickerId => AssetType == AssetType.Ticker
            ? TickerId.FromUInt32(_value)
            : throw new InvalidOperationException($"Tried to retrieve the TickerId of a {QueryType} query event");

        public SwitchId SwitchId => AssetType == AssetType.Switch
            ? SwitchId.FromUInt32(_value)
            : throw new InvalidOperationException($"Tried to retrieve the SwitchId of a {QueryType} query event");

        public MapId MapId => AssetType == AssetType.Map
            ? MapId.FromUInt32(_value)
            : throw new InvalidOperationException($"Tried to retrieve the MapId of a {QueryType} query event");

        public string StringValue => (AssetType, QueryType) switch
        {
            (AssetType.Item, _) => ItemId.ToString(),
            (AssetType.PartyMember, _) => PartyMemberId.ToString(),
            (AssetType.Ticker, _) => TickerId.ToString(),
            (AssetType.Switch, _) => SwitchId.ToString(),
            (AssetType.None, QueryType.ChosenVerb) => ((TriggerType)_value).ToString(),
            (AssetType.None, QueryType.TriggerType) => ((TriggerType)_value).ToString(),
            _ => _value.ToString(CultureInfo.InvariantCulture)
        };

        public override string ToString() => $"query {QueryType} {StringValue} ({Operation} {Immediate})";
        public override MapEventType EventType => MapEventType.Query;

        public static QueryEvent TemporarySwitch(SwitchId id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.TemporarySwitch,
            _value = id.ToUInt32(),
            Operation = operation,
            Immediate = immediate
        };

        public static QueryEvent HasPartyMember(PartyMemberId id) => new QueryEvent { QueryType = QueryType.HasPartyMember, _value = id.ToUInt32() };
        public static QueryEvent InventoryHasItem(ItemId id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.InventoryHasItem,
            _value = id.ToUInt32(),
            Operation = operation,
            Immediate = immediate
        };

        public static QueryEvent UsedItemId(ItemId id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.UsedItemId,
            _value = id.ToUInt32(),
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
        public static QueryEvent IsNpcActive(NpcId id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.IsNpcActive,
            _value = id.ToUInt32(),
            Operation = operation,
            Immediate = immediate
        };
        public static QueryEvent HasEnoughGold(ushort amount) => new QueryEvent { QueryType = QueryType.HasEnoughGold, _value = amount };
        public static QueryEvent RandomChance(ushort percentage) => new QueryEvent { QueryType = QueryType.RandomChance, _value = percentage, };
        public static QueryEvent IsPartyMemberConscious(PartyMemberId id) => new QueryEvent { QueryType = QueryType.IsPartyMemberConscious, _value = id.ToUInt32(), };
        public static QueryEvent IsPartyMemberLeader(PartyMemberId id) => new QueryEvent { QueryType = QueryType.IsPartyMemberLeader, _value = id.ToUInt32(), };
        public static QueryEvent Ticker(TickerId id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.Ticker,
            _value = id.ToUInt32(),
            Operation = operation,
            Immediate = immediate
        };
        public static QueryEvent CurrentMapId(MapId id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.CurrentMapId,
            _value = id.ToUInt32(),
            Operation = operation,
            Immediate = immediate
        };

        public static QueryEvent PromptPlayer(ushort textId, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.PromptPlayer,
            _value = textId,
            Operation = operation,
            Immediate = immediate
        };

        public static QueryEvent TriggerType(TriggerTypes id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.TriggerType,
            _value = (uint)id,
            Operation = operation,
            Immediate = immediate
        };

        public static QueryEvent EventAlreadyUsed(ushort id, QueryOperation operation, byte immediate) => new QueryEvent
        {
            QueryType = QueryType.EventAlreadyUsed,
            _value = id,
            Operation = operation,
            Immediate = immediate
        };

        public static QueryEvent IsDemoVersion() => new QueryEvent { QueryType = QueryType.IsDemoVersion, };
        public static QueryEvent PromptPlayerNumeric(ushort value) => new QueryEvent { QueryType = QueryType.PromptPlayerNumeric, _value = value, };
    }
/*
    public class QueryItemEvent : MapEvent
    {
        QueryItemEvent(QueryType subType)
        {
            QueryType = subType;
        }

        public static QueryItemEvent Serdes(QueryItemEvent e, ISerializer s, QueryType subType)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new QueryItemEvent(subType);
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.ItemId = s.EnumU16(nameof(ItemId), e.ItemId);

            ApiUtil.Assert(e.Unk4 == 0);
            ApiUtil.Assert(e.Unk5 == 0);

            return e;
        }

        public QueryOperation Operation { get; private set; }
        public byte Immediate { get; private set; } // immediate value?
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        public ItemId ItemId { get; private set; }

        public override string ToString() => $"query_item {QueryType} {ItemId} {Operation} {Immediate}";
        public override MapEventType EventType => MapEventType.Query;
        public QueryType QueryType { get; }
    }
    public class QueryPartyEvent : MapEvent, IQueryEvent
    {
        public static QueryPartyEvent Serdes(QueryPartyEvent e, ISerializer s, QueryType subType)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new QueryPartyEvent();
            e.QueryType = subType;
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.PartyMemberId = s.TransformEnumU8(nameof(PartyMemberId), e.PartyMemberId, ZeroToNullConverter<PartyCharacterId>.Instance);
            s.UInt8("pad", 0);
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
    public class PromptPlayerNumericEvent : MapEvent, IQueryEvent
    {
        public static PromptPlayerNumericEvent Serdes(PromptPlayerNumericEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new PromptPlayerNumericEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            s.UInt16("Padding", 0);
            e.Argument = s.UInt16(nameof(Argument), e.Argument);
            return e;
        }

        public QueryType QueryType => QueryType.PromptPlayerNumeric;
        public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Immediate { get; private set; } // immediate value?
        public ushort Argument { get; private set; }

        public override string ToString() => $"query {QueryType} {Argument} ({Operation} {Immediate})";
        public override MapEventType EventType => MapEventType.Query;
    }
    public class PromptPlayerEvent : MapEvent, IQueryEvent, ITextEvent
    {
        public static PromptPlayerEvent Serdes(PromptPlayerEvent e, ISerializer s, TextId textSourceId)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new PromptPlayerEvent(textSourceId);
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            s.UInt16("Padding", 0);
            e.TextId = s.UInt16(nameof(TextId), e.TextId);
            return e;
        }

        PromptPlayerEvent(TextId textSourceId)
        {
            TextSourceId = textSourceId;
        }

        public QueryType QueryType => QueryType.PromptPlayer;
        public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Immediate { get; private set; } // immediate value?
        public ushort TextId { get; private set; }

        public override string ToString() => $"query {QueryType} {TextId} ({Operation} {Immediate})";
        public override MapEventType EventType => MapEventType.Query;

        public TextId TextSourceId { get; }
    }*/
}
