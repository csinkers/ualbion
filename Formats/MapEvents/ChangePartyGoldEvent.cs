using SerdesNet;

namespace UAlbion.Formats.MapEvents
{
    public class ChangePartyGoldEvent : ModifyEvent
    {
        public static ChangePartyGoldEvent Serdes(ChangePartyGoldEvent e, ISerializer s)
        {
            e ??= new ChangePartyGoldEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Amount = s.UInt16(nameof(Amount), e.Amount);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }

        public QuantityChangeOperation Operation { get; private set; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Amount { get; private set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"change_party_gold {Operation} {Amount} ({Unk3} {Unk4} {Unk5} {Unk8})";
        public override ModifyType SubType => ModifyType.ChangePartyGold;
    }
}
