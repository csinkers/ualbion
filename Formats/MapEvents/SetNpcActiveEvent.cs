using System.IO;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class SetNpcActiveEvent : ModifyEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type, ModifyType subType)
        {
            return new EventNode(id, new SetNpcActiveEvent
            {
                IsActive = br.ReadByte(), // 2
                NpcId = (NpcCharacterId) br.ReadByte() - 1, // 3
                Unk4 = br.ReadByte(), // 4
                Unk5 = br.ReadByte(), // 5
                Unk6 = br.ReadUInt16(), // 6
                Unk8 = br.ReadUInt16(), // 8
            });
        }

        public byte IsActive { get; private set; }
        public NpcCharacterId NpcId { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"set_npc_active {NpcId} {IsActive} ({Unk4} {Unk6} {Unk8})";
    }
}
