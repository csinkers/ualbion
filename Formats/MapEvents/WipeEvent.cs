using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class WipeEvent : IMapEvent
    {
        public static WipeEvent Translate(WipeEvent node, ISerializer s)
        {
            node ??= new WipeEvent();
            s.Dynamic(node, nameof(Value));
            s.Dynamic(node, nameof(Unk2));
            s.Dynamic(node, nameof(Unk3));
            s.Dynamic(node, nameof(Unk4));
            s.Dynamic(node, nameof(Unk5));
            s.Dynamic(node, nameof(Unk6));
            s.Dynamic(node, nameof(Unk8));
            Debug.Assert(node.Unk2 == 0);
            Debug.Assert(node.Unk3 == 0);
            Debug.Assert(node.Unk4 == 0);
            Debug.Assert(node.Unk5 == 0);
            Debug.Assert(node.Unk6 == 0);
            Debug.Assert(node.Unk8 == 0);
            return node;
        }

        public byte Value { get; private set; }
        byte Unk2 { get; set; }
        byte Unk3 { get; set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        ushort Unk6 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"wipe {Value}";
        public MapEventType EventType => MapEventType.Wipe;
    }
}
