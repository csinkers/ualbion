using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("remove_party_member", "Remove someone from the party", "rpm")]
public class RemovePartyMemberEvent : MapEvent
{
    public static RemovePartyMemberEvent Serdes(RemovePartyMemberEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new RemovePartyMemberEvent();
        e.PartyMemberId = PartyMemberId.SerdesU8(nameof(PartyMemberId), e.PartyMemberId, mapping, s);
        e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
        zeroes += s.UInt16(null, 0);
        s.Assert(zeroes == 0, "RemovePartyMemberEvent: Expected fields 3,4,5,8 to be 0");
        return e;
    }

    RemovePartyMemberEvent() { }
    public RemovePartyMemberEvent(PartyMemberId partyMemberId, byte unk2, ushort unk6)
    {
        PartyMemberId = partyMemberId;
        Unk2 = unk2;
        Unk6 = unk6;
    }

    [EventPart("member_id")] public PartyMemberId PartyMemberId { get; private set; }
    [EventPart("unk2", true, (byte)0)] public byte Unk2 { get; private set; }
    [EventPart("unk6", true, (ushort)0)] public ushort Unk6 { get; private set; }
    public override MapEventType EventType => MapEventType.RemovePartyMember;
}