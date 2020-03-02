using UAlbion.Api;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class DisableEventChainEvent : ModifyEvent
    {
        public static DisableEventChainEvent Serdes(DisableEventChainEvent e, ISerializer s)
        {
            e ??= new DisableEventChainEvent();
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.ChainNumber = s.UInt8(nameof(ChainNumber), e.ChainNumber);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            ApiUtil.Assert(e.Unk2 == 1 || e.Unk2 == 0 || e.Unk2 == 2); // Usually 1
            return e;
        }

        public byte Unk2 { get; private set; }
        public byte ChainNumber { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"disable_event_chain {ChainNumber} ({Unk2} {Unk4} {Unk5} {Unk6} {Unk8})";
        public override ModifyType SubType => ModifyType.DisableEventChain;
    }
}
