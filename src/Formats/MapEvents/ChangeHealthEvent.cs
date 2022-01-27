using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("change_health")]
public class ChangeHealthEvent : DataChangeEvent
{
    ChangeHealthEvent() { }
    public ChangeHealthEvent(PartyMemberId partyMemberId, NumericOperation operation, ushort amount, byte unk3, ushort unk6)
    {
        PartyMemberId = partyMemberId;
        Operation = operation;
        Amount = amount;
        Unk3 = unk3;
        Unk6 = unk6;
    }

    public static ChangeHealthEvent Serdes(ChangeHealthEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new ChangeHealthEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
        int zeroed = s.UInt8(null, 0);
        s.Assert(zeroed == 0, "ChangeHealthEvent: Expected byte 4 to be 0");
        e.PartyMemberId = PartyMemberId.SerdesU8(nameof(PartyMemberId), e.PartyMemberId, mapping, s);
        e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
        e.Amount = s.UInt16(nameof(Amount), e.Amount);
        s.Assert(zeroed == 0, "ChangeHealthEvent: Expected word 6 to be 0");
        return e;
    }
    public override ChangeProperty ChangeProperty => ChangeProperty.Health;
    [EventPart("party_member")] public PartyMemberId PartyMemberId { get; private set; }
    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("amount")] public ushort Amount { get; private set; }
    [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; }
    [EventPart("unk6", true, (ushort)0)] public ushort Unk6 { get; private set; }
}