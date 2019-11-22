using System.IO;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class SetPartyLeaderEvent : ModifyEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type, ModifyType subType)
        {
            return new EventNode(id, new SetPartyLeaderEvent
            {
                Unk2 = br.ReadByte(), // 2
                Unk3 = br.ReadByte(), // 3
                Unk4 = br.ReadByte(), // 4
                Unk5 = br.ReadByte(), // 5
                PartyMemberId = (PartyCharacterId) br.ReadUInt16(), // 6
                Unk8 = br.ReadUInt16(), // 8
            });
        }

        public byte Unk2 { get; private set; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public PartyCharacterId PartyMemberId { get; private set; } // stored as ushort
        public ushort Unk8 { get; set; }
        public override string ToString() => $"set_party_leader {PartyMemberId} ({Unk2} {Unk3} {Unk4} {Unk5} {Unk8})";
    }
}
