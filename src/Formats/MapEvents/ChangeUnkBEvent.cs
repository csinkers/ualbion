using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("change_unkb")]
    public class ChangeUnkBEvent : DataChangeEvent
    {
        ChangeUnkBEvent() { }
        public ChangeUnkBEvent(byte unk5, NumericOperation operation, ushort amount, ushort unk6, byte unk3)
        {
            Unk5 = unk5;
            Operation = operation;
            Amount = amount;
            Unk6 = unk6;
            Unk3 = unk3;
        }

        public static ChangeUnkBEvent Serdes(ChangeUnkBEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new ChangeUnkBEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            int zeroed = s.UInt8(null, 0);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Amount = s.UInt16(nameof(Amount), e.Amount);
            s.Assert(zeroed == 0, "ChangeUnkBEEvent: Expected byte 4 to be 0");
            return e;
        }
        public override ChangeProperty ChangeProperty => ChangeProperty.UnkB;
        [EventPart("unk5")] public byte Unk5 { get; private set; }
        [EventPart("op")] public NumericOperation Operation { get; private set; }
        [EventPart("amount")] public ushort Amount { get; private set; }
        [EventPart("unk6")] public ushort Unk6 { get; private set; }
        [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; }
    }
}
