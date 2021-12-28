using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("add_party_member", "Add someone to the party", "apm")]
public class AddPartyMemberEvent : ModifyEvent
{
    public static AddPartyMemberEvent Serdes(AddPartyMemberEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new AddPartyMemberEvent();
        e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
        int check = s.UInt8(null, 0);
        check += s.UInt8(null, 0);
        e.PartyMemberId = PartyMemberId.SerdesU8(nameof(PartyMemberId), e.PartyMemberId, mapping, s);
        s.UInt8("pad", 0);
        check += s.UInt16(null,0);

        s.Assert(check == 0, "Expected fields 4, 5 and 8 of AddPartyMemberEvent to be 0");
        return e;
    }

    public AddPartyMemberEvent(PartyMemberId partyMemberId, byte unk2, byte unk3)
    {
        PartyMemberId = partyMemberId;
        Unk2 = unk2;
        Unk3 = unk3;
    }

    AddPartyMemberEvent() { }

    [EventPart("member_id")] public PartyMemberId PartyMemberId { get; private set; }
    [EventPart("unk2", true, (byte)0)] public byte Unk2 { get; private set; }
    [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; }
    public override ModifyType SubType => ModifyType.AddPartyMember;
}