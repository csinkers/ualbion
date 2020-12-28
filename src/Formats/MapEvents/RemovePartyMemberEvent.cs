using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("remove_party_member", "Remove someone from the party", "rpm")]
    public class RemovePartyMemberEvent : MapEvent
    {
        public static RemovePartyMemberEvent Serdes(RemovePartyMemberEvent e, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new RemovePartyMemberEvent();
            e.PartyMemberId = PartyMemberId.SerdesU8(nameof(PartyMemberId), e.PartyMemberId, mapping, s);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            ApiUtil.Assert(e.Unk4 == 0);
            ApiUtil.Assert(e.Unk5 == 0);
            ApiUtil.Assert(e.Unk8 == 0);
            return e;
        }

        public RemovePartyMemberEvent(PartyMemberId partyMemberId) { PartyMemberId = partyMemberId;}
        RemovePartyMemberEvent() { }

        [EventPart("member_id")]
        public PartyMemberId PartyMemberId { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"remove_party_member ({PartyMemberId} {Unk2} {Unk3} {Unk6})";
        public override MapEventType EventType => MapEventType.RemovePartyMember;
    }
}
