using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class SetMapLightingEvent : ModifyEvent
    {
        public enum LightingLevel
        {
            Normal = 0,
            NeedTorch = 1,
            FadeFromBlack = 2
        }

        public SetMapLightingEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Unk2 = br.ReadByte(); // 2
            Unk3 = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            LightLevel = (LightingLevel)br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public byte Unk2 { get; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public LightingLevel LightLevel { get; }
        public ushort Unk8 { get; set; }
    }
}