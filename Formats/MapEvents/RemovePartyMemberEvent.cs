using System.IO;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class RemovePartyMemberEvent : IEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type)
        {
            return new EventNode(id, new RemovePartyMemberEvent
            {
                Unk1 = br.ReadByte(), // +1
                Unk2 = br.ReadByte(), // +2
                Unk3 = br.ReadByte(), // +3
                Unk4 = br.ReadByte(), // +4
                Unk5 = br.ReadByte(), // +5
                //Unk6 = br.ReadUInt16(), // +6
                PartyMemberId = (PartyCharacterId)br.ReadUInt16(), // TODO: Verify
                Unk8 = br.ReadUInt16(), // +8
            });
        }

        public RemovePartyMemberEvent(PartyCharacterId partyMemberId) { PartyMemberId = partyMemberId;}
        RemovePartyMemberEvent() { }

        [EventPart("member_id")] public PartyCharacterId PartyMemberId { get; private set; }

        public byte Unk1 { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; }
        // public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"remove_party_member ({PartyMemberId} {Unk1} {Unk2} {Unk3} {Unk4} {Unk5} {Unk8})";
    }
}
