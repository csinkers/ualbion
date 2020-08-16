using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("add_party_member")]
    public class AddPartyMemberEvent : ModifyEvent
    {
        public static AddPartyMemberEvent Serdes(AddPartyMemberEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new AddPartyMemberEvent();
            s.Begin();
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.PartyMemberId = (PartyCharacterId)StoreIncremented.Serdes(nameof(PartyMemberId), (ushort)e.PartyMemberId, s.UInt16);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            s.End();
            return e;
        }

        public AddPartyMemberEvent(PartyCharacterId partyMemberId) { PartyMemberId = partyMemberId; }
        AddPartyMemberEvent() { }

        [EventPart("member_id")]
        public PartyCharacterId PartyMemberId { get; private set; }

        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"add_party_member {PartyMemberId} ({Unk2} {Unk3} {Unk4} {Unk5} {Unk8})";
        public override ModifyType SubType => ModifyType.AddPartyMember;
    }
}
