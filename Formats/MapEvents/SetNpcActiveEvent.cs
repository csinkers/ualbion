using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class SetNpcActiveEvent : ModifyEvent
    {
        public static SetNpcActiveEvent Translate(SetNpcActiveEvent e, ISerializer s)
        {
            e ??= new SetNpcActiveEvent();
            s.Dynamic(e, nameof(IsActive));
            e.NpcId = s.EnumU8(nameof(NpcId), e.NpcId);
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(Unk6));
            s.Dynamic(e, nameof(Unk8));
            return e;
        }

        public byte IsActive { get; private set; }
        public NpcCharacterId NpcId { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"set_npc_active {NpcId} {IsActive} ({Unk4} {Unk6} {Unk8})";
        public override ModifyType SubType => ModifyType.SetNpcActive;
    }
}
