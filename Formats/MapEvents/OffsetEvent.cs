using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class OffsetEvent : IMapEvent
    {
        public static OffsetEvent Translate(OffsetEvent e, ISerializer s)
        {
            e ??= new OffsetEvent();
            s.Dynamic(e, nameof(X));
            s.Dynamic(e, nameof(Y));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(Unk6));
            s.Dynamic(e, nameof(Unk8));
            Debug.Assert(e.Unk3 == 1 || e.Unk3 == 3);
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk6 == 0);
            Debug.Assert(e.Unk8 == 0);
            return e;
        }

        public sbyte X { get; private set; }
        public sbyte Y { get; private set; }
        public byte Unk3 { get; private set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        ushort Unk6 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"offset <{X}, {Y}> ({Unk3} {Unk4} {Unk5} {Unk6} {Unk8})";
        public MapEventType EventType => MapEventType.Offset;
    }
}
