using SerdesNet;

namespace UAlbion.Formats.MapEvents
{
    public class QueryVerbEvent : MapEvent, IQueryEvent
    {
        public enum VerbType : ushort
        {
            Normal     =  0,
            Examine    =  1,
            Manipulate =  2,
            TalkTo     =  3,
            UseItem    =  4,
            MapInit    =  5,
            EveryStep  =  6,
            EveryHour  =  7,
            EveryDay   =  8,
            Default    =  9,
            Action     = 10,
            Npc        = 11,
            Take       = 12,
            Unk13      = 13,
            Unk14      = 14,
            Unk15      = 15,
        }

        public static QueryVerbEvent Serdes(QueryVerbEvent e, ISerializer s)
        {
            e ??= new QueryVerbEvent();
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Verb = s.EnumU16(nameof(Verb), e.Verb);
            return e;
        }

        public VerbType Verb { get; private set; }
        public byte Unk2 { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Unk3 { get; private set; } // immediate value?
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }

        public override string ToString() => $"query_verb {Verb} (method {Unk2})";
        public override MapEventType EventType => MapEventType.Query;
        public QueryType QueryType => QueryType.ChosenVerb;
    }
}
