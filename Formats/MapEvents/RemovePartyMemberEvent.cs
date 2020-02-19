using System.Diagnostics;
using System.IO;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class RemovePartyMemberEvent : IEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type)
        {
            var e = new RemovePartyMemberEvent
            {
                PartyMemberId = (PartyCharacterId) br.ReadByte(), // +1
                Unk2 = br.ReadByte(), // +2
                Unk3 = br.ReadByte(), // +3
                Unk4 = br.ReadByte(), // +4
                Unk5 = br.ReadByte(), // +5
                Unk6 = br.ReadUInt16(), // +6
                Unk8 = br.ReadUInt16(), // +8
            };
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk8 == 0);
            return new EventNode(id, e);
        }

        public RemovePartyMemberEvent(PartyCharacterId partyMemberId) { PartyMemberId = partyMemberId;}
        RemovePartyMemberEvent() { }

        public PartyCharacterId PartyMemberId { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"remove_party_member ({PartyMemberId} {Unk2} {Unk3} {Unk6})";
    }
}
