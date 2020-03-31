using SerdesNet;

namespace UAlbion.Formats.MapEvents
{
    public class PlaceActionEvent : MapEvent
    {
        public static PlaceActionEvent Serdes(PlaceActionEvent e, ISerializer s)
        {
            e ??= new PlaceActionEvent();
            e.Type = s.EnumU8(nameof(Type), e.Type);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }

        public PlaceActionType Type { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; } // Spell class for learn magic
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"place_action {Type} ({Unk2} {Unk3} {Unk4} {Unk5} {Unk6} {Unk8})";
        public override MapEventType EventType => MapEventType.PlaceAction;
    }
}
