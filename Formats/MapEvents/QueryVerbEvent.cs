namespace UAlbion.Formats.MapEvents
{
    public class QueryVerbEvent : QueryEvent
    {
        public enum VerbType : byte
        {
            Examine = 1,
            Manipulate = 2,
            Speak = 3,
            UseItem = 4,
        }
        public QueryVerbEvent(int id, EventType type) : base(id, type) { }
        public VerbType Verb => (VerbType) Argument;
        public override string ToString() => $"query_verb {SubType} {Verb} (method {Unk2})";
    }
}