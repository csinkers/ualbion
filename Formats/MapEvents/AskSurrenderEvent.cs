using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class AskSurrenderEvent : IMapEvent
    {
        public static AskSurrenderEvent Serdes(AskSurrenderEvent node, ISerializer s)
        {
            node ??= new AskSurrenderEvent();
            s.Dynamic(node, nameof(Unk1));
            s.Dynamic(node, nameof(Unk2));
            s.Dynamic(node, nameof(Unk3));
            s.Dynamic(node, nameof(Unk4));
            s.Dynamic(node, nameof(Unk5));
            s.Dynamic(node, nameof(Unk6));
            s.Dynamic(node, nameof(Unk8));
            return node;
        }

        public byte Unk1 { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }
        public MapEventType EventType => MapEventType.AskSurrender;
    }
}
