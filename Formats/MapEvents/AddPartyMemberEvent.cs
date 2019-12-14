using System.IO;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("add_party_member")]
    public class AddPartyMemberEvent : ModifyEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type, ModifyType subType)
        {
            return new EventNode(id, new AddPartyMemberEvent
            {
                SubType = subType,
                Unk2 = br.ReadByte(), // 2
                Unk3 = br.ReadByte(), // 3
                Unk4 = br.ReadByte(), // 4
                Unk5 = br.ReadByte(), // 5
                PartyMemberId = (PartyCharacterId)br.ReadUInt16(), // 6
                Unk8 = br.ReadUInt16(), // 8
            });
        }

        public AddPartyMemberEvent(PartyCharacterId partyMemberId) { PartyMemberId = partyMemberId;}
        AddPartyMemberEvent() { }

        [EventPart("member_id")]
        public PartyCharacterId PartyMemberId { get; private set; }

        public byte Unk2 { get; private set; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"add_party_member {PartyMemberId} ({Unk2} {Unk3} {Unk4} {Unk5} {Unk8})";
    }
}
