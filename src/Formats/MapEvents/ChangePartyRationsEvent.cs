using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("change_party_rations")]
    public class ChangePartyRationsEvent : ModifyEvent
    {
        public static ChangePartyRationsEvent Serdes(ChangePartyRationsEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new ChangePartyRationsEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            int zeroes = s.UInt8(null, 0);
            zeroes += s.UInt8(null, 0);
            e.Amount = s.UInt16(nameof(Amount), e.Amount);
            zeroes += s.UInt16(null, 0);
            s.Assert(zeroes == 0, "ChangePartyRations: Expected fields 4,5,8 to be 0");
            return e;
        }

        ChangePartyRationsEvent() { }

        public ChangePartyRationsEvent(NumericOperation operation, ushort amount, byte unk3)
        {
            Operation = operation;
            Amount = amount;
            Unk3 = unk3;
        }

        [EventPart("op")] public NumericOperation Operation { get; private set; }
        [EventPart("amount")] public ushort Amount { get; private set; }
        [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; }
        public override ModifyType SubType => ModifyType.PartyRations;
    }
}
