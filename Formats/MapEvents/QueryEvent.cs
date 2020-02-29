using System.Diagnostics;
using UAlbion.Api;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class QueryEvent : Event, IQueryEvent
    {
        public static IQueryEvent Serdes(IQueryEvent genericEvent, ISerializer s)
        {
            var subType = s.EnumU8("SubType", genericEvent?.QueryType ?? QueryType.IsScriptDebugModeActive);
            switch (subType)
            {
                case QueryType.InventoryHasItem:
                case QueryType.UsedItemId:
                    return QueryItemEvent.Serdes((QueryItemEvent)genericEvent, s, subType);

                case QueryType.ChosenVerb:
                    return QueryVerbEvent.Serdes((QueryVerbEvent)genericEvent, s);

                case QueryType.PreviousActionResult:
                case QueryType.Ticker:
                case QueryType.CurrentMapId:
                case QueryType.PromptPlayer:
                case QueryType.TriggerType:
                default:
                    break;
            }

            var e = (QueryEvent)genericEvent ?? new QueryEvent { SubType = subType };
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Argument = s.UInt16(nameof(Argument), e.Argument);
            e.FalseEventId = ConvertMaxToNull.Serdes(nameof(FalseEventId), e.FalseEventId, s.UInt16);

            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);

            return e;
        }

        public enum QueryOperation
        {
            Unk0,
            Unk1,
            Unk2,
            Equals,
        }

        public byte Unk2 { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Unk3 { get; private set; } // immediate value?
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }

        public QueryType SubType { get; private set; }
        public ushort Argument { get; private set; }

        public override string ToString() => $"query {SubType} {Argument} ({Unk2} {Unk3})";
        public MapEventType EventType => MapEventType.Query;
        public QueryType QueryType { get; }
        public ushort? FalseEventId { get; set; }
    }
}
