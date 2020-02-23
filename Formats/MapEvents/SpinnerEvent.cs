using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class SpinnerEvent : IMapEvent
    {
        public static SpinnerEvent Serdes(SpinnerEvent e, ISerializer s)
        {
            e ??= new SpinnerEvent();
            e.Unk1 = s.UInt8(nameof(Unk1), e.Unk1);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            Debug.Assert(e.Unk2 == 0);
            Debug.Assert(e.Unk3 == 0);
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk6 == 0);
            Debug.Assert(e.Unk8 == 0);
            return e;
        }

        public byte Unk1 { get; private set; }
        byte Unk2 { get; set; }
        byte Unk3 { get; set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        ushort Unk6 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"spinner {Unk1}";
        public MapEventType EventType => MapEventType.Spinner;
    }
}
