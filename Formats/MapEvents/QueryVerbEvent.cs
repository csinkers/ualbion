using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class QueryVerbEvent : IQueryEvent
    {
        public enum VerbType : ushort
        {
            Unk0 = 0,
            Examine = 1,
            Manipulate = 2,
            TalkTo = 3,
            UseItem = 4,
            Unk5 = 5,
            Unk6 = 6,
            Unk8 = 8,
            Unk11 = 11,
        }

        public static QueryVerbEvent Translate(QueryVerbEvent e, ISerializer s)
        {
            e ??= new QueryVerbEvent();
            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.EnumU16(nameof(Verb), () => e.Verb, x => e.Verb = x, x => ((ushort)x, x.ToString()));

            s.UInt16(nameof(FalseEventId),
                () => e.FalseEventId ?? 0xffff,
                x => e.FalseEventId = x == 0xffff ? (ushort?)null : x);

            return e;
        }

        public VerbType Verb { get; set; }
        public byte Unk2 { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Unk3 { get; private set; } // immediate value?
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }

        public override string ToString() => $"query_verb {Verb} (method {Unk2})";
        public MapEventType EventType => MapEventType.Query;
        public QueryType QueryType => QueryType.ChosenVerb;
        public ushort? FalseEventId { get; set; }
    }
}
