using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class DummyModifyEvent : ModifyEvent
    {
        public static DummyModifyEvent Translate(DummyModifyEvent e, ISerializer s)
        {
            e ??= new DummyModifyEvent();
            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(Unk6));
            s.Dynamic(e, nameof(Unk8));
            return e;
        }

        public byte Unk2 { get; private set; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; set; }
        public override ModifyType SubType => ModifyType.Unk2;
    }
}
