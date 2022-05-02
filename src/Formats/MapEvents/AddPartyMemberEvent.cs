using System;
using SerdesNet;
using UAlbion.Api.Eventing;
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
        int op = s.UInt8("b2", 1);
        s.Assert(op == 1, "Expected AddPartyMember operation to be SetToMaximum");

        int zeroed = s.UInt8("b3", 0);
        zeroed += s.UInt8("b4", 0);
        zeroed += s.UInt8("b5", 0);
        e.PartyMemberId = PartyMemberId.SerdesU16(nameof(PartyMemberId), e.PartyMemberId, mapping, s);
        zeroed += s.UInt16("w8", 0);
        s.Assert(zeroed == 0, "Expected fields 3-5, 8 of AddPartyMemberEvent to be 0");
        return e;
    }

    AddPartyMemberEvent() { }
    public AddPartyMemberEvent(PartyMemberId partyMemberId) => PartyMemberId = partyMemberId;

    [EventPart("id")] public PartyMemberId PartyMemberId { get; private set; }
    public override ModifyType SubType => ModifyType.AddPartyMember;
}