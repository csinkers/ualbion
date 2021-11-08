using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("change_unk0")]
    public class ChangeUnk0Event : DataChangeEvent
    {
        ChangeUnk0Event() { }
        public ChangeUnk0Event(NumericOperation operation, ushort amount, ushort unk6, byte unk3)
        {
            Operation = operation;
            Amount = amount;
            Unk6 = unk6;
            Unk3 = unk3;
        }

        public static ChangeUnk0Event Serdes(ChangeUnk0Event e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new ChangeUnk0Event();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            int zeroed = s.UInt8(null, 0);
            zeroed += s.UInt8(null, 0);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Amount = s.UInt16(nameof(Amount), e.Amount);
            s.Assert(zeroed == 0, "ChangeUnk0Event: Expected bytes 4, 5 to be 0");
            return e;
        }

        public override ChangeProperty ChangeProperty => ChangeProperty.Unk0;
        [EventPart("op")] public NumericOperation Operation { get; private set; }
        [EventPart("amount")] public ushort Amount { get; private set; }
        [EventPart("unk6")] public ushort Unk6 { get; private set; }
        [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; }
    }
}
