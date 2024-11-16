using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("add_party_member", "Add someone to the party", "apm")]
public class AddPartyMemberEvent : ModifyEvent
{
    public static AddPartyMemberEvent Serdes(AddPartyMemberEvent e, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new AddPartyMemberEvent();
        e.Operation = s.EnumU8(nameof(e.Operation), e.Operation);
        if (e.Operation != NumericOperation.SetAmount)
            s.Assert(false, $"Expected AddPartyMember operation to be SetAmount (3), but it was {e.Operation} ({(int)e.Operation})");

        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
        if (e.Unk3 > 10)
            s.Assert(false, $"Expected field 3 of AddPartyMemberEvent to be in the range [0..10], but it was {e.Unk3}");

        int temp = s.UInt8("b4", 0);
        if (temp != 0)
            s.Assert(false, $"Expected field 4 of AddPartyMemberEvent to be 0, but it was {temp}");

        temp = s.UInt8("b5", 0);
        if (temp != 0) 
            s.Assert(false, $"Expected field 5 of AddPartyMemberEvent to be 0, but it was {temp}");

        e.PartyMemberId = PartyMemberId.SerdesU16(nameof(PartyMemberId), e.PartyMemberId, mapping, s);

        temp = s.UInt16("w8", 0);
        if (temp != 0) 
            s.Assert(false, $"Expected field 8 of AddPartyMemberEvent to be 0, but it was {temp}");

        return e;
    }

    AddPartyMemberEvent() { }
    public AddPartyMemberEvent(PartyMemberId partyMemberId, NumericOperation op, byte unk3)
    {
        Operation = op;
        PartyMemberId = partyMemberId;
        Unk3 = unk3;
    }

    [EventPart("id")] public PartyMemberId PartyMemberId { get; private set; }
    [EventPart("op", true, NumericOperation.SetAmount)] public NumericOperation Operation { get; private set; }
    [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; }
    public override ModifyType SubType => ModifyType.AddPartyMember;
}