using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class SetNpcActiveEvent : ModifyEvent
    {
        public static SetNpcActiveEvent Serdes(SetNpcActiveEvent e, ISerializer s)
        {
            e ??= new SetNpcActiveEvent();
            e.IsActive = s.UInt8(nameof(IsActive), e.IsActive);
            e.NpcId = s.UInt8(nameof(NpcId), e.NpcId);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }

        public byte IsActive { get; private set; }
        public byte NpcId { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"set_npc_active {NpcId} {IsActive} ({Unk4} {Unk6} {Unk8})";
        public override ModifyType SubType => ModifyType.SetNpcActive;
    }
}
