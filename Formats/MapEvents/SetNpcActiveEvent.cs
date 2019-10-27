using System.IO;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class SetNpcActiveEvent : ModifyEvent
    {
        public SetNpcActiveEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            IsActive = br.ReadByte(); // 2
            NpcId = (NpcCharacterId)br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            Unk6 = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public byte IsActive { get; }
        public NpcCharacterId NpcId { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk6 { get; }
        public ushort Unk8 { get; set; }
    }
}