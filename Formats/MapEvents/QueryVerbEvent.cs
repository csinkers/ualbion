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

        public static QueryVerbEvent Serdes(QueryVerbEvent e, ISerializer s)
        {
            e ??= new QueryVerbEvent();
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Verb = s.EnumU16(nameof(Verb), e.Verb);
            e.FalseEventId = ConvertMaxToNull.Serdes(nameof(FalseEventId), e.FalseEventId, s.UInt16);
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
