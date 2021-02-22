using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("npc_active")]
    public class NpcActiveEvent : ModifyEvent
    {
        NpcActiveEvent() { }

        public NpcActiveEvent(byte npcIndex, byte isActive, byte unk5, ushort unk6)
        {
            NpcIndex = npcIndex;
            IsActive = isActive;
            Unk5 = unk5;
            Unk6 = unk6;
        }

        public static NpcActiveEvent Serdes(NpcActiveEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new NpcActiveEvent();
            e.IsActive = s.UInt8(nameof(IsActive), e.IsActive);
            e.NpcIndex = s.UInt8(nameof(NpcIndex), e.NpcIndex);
            int zeroed = s.UInt8(null, 0);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            zeroed += s.UInt16(null, 0);
            s.Assert(zeroed == 0, "NpcActiveEvent:Expected fields 4,8 to be 0");
            return e;
        }

        [EventPart("")] public byte NpcIndex { get; private set; }
        [EventPart("")] public byte IsActive { get; private set; }
        [EventPart("")] public byte Unk5 { get; private set; }
        [EventPart("")] public ushort Unk6 { get; private set; }
        public override ModifyType SubType => ModifyType.NpcActive;
    }
}
