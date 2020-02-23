using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class QueryEvent : IQueryEvent
    {
        public static IQueryEvent Serdes(IQueryEvent genericEvent, ISerializer s)
        {
            var subType = s.EnumU8("SubType", genericEvent?.QueryType ?? QueryType.IsScriptDebugModeActive);
            switch (subType)
            {
                case QueryType.InventoryHasItem:
                case QueryType.UsedItemId:
                    return QueryItemEvent.Translate((QueryItemEvent)genericEvent, s, subType);

                case QueryType.ChosenVerb:
                    return QueryVerbEvent.Translate((QueryVerbEvent)genericEvent, s);

                case QueryType.PreviousActionResult:
                case QueryType.Ticker:
                case QueryType.CurrentMapId:
                case QueryType.PromptPlayer:
                case QueryType.TriggerType:
                default:
                    break;
            }

            var e = (QueryEvent)genericEvent ?? new QueryEvent { SubType = subType };
            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(Argument));
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
