using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("change_experience")]
public class ChangeExperienceEvent : DataChangeEvent
{
    ChangeExperienceEvent() { }
    public ChangeExperienceEvent(PartyMemberId partyMemberId, NumericOperation operation, ushort amount, byte unk3)
    {
        PartyMemberId = partyMemberId;
        Operation = operation;
        Amount = amount;
        Unk3 = unk3;
    }

    public static ChangeExperienceEvent Serdes(ChangeExperienceEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new ChangeExperienceEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
        int zeroed = s.UInt8(null, 0);
        e.PartyMemberId = PartyMemberId.SerdesU8(nameof(PartyMemberId), e.PartyMemberId, mapping, s);
        zeroed += s.UInt16(null, 0);
        e.Amount = s.UInt16(nameof(Amount), e.Amount);
        s.Assert(zeroed == 0, "ChangeExperienceEvent: Expected byte 4 to be 0");
        return e;
    }
    public override ChangeProperty ChangeProperty => ChangeProperty.Experience;
    [EventPart("party_member")] public PartyMemberId PartyMemberId { get; private set; }
    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("amount")] public ushort Amount { get; private set; }
    [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; }
}