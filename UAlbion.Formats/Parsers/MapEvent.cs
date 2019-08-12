using System.IO;

namespace UAlbion.Formats.Parsers
{
    public class MapEvent
    {
        public enum EventType : byte
        {
            Script = 0,
            MapExit = 1,
            Door = 2,
            Chest = 3,
            Text = 4,
            Spinner = 5,
            Trap = 6,
            ChangeUsedItem = 7,
            DataChange = 8,
            ChangeIcon = 9,
            Encounter = 0xA,
            PlaceAction = 0xB,
            Query = 0xC,
            Modify = 0xD,
            Action = 0xE,
            Signal = 0xF,
            CloneAutomap = 0x10,
            Sound = 0x11,
            StartDialogue = 0x12,
            CreateTransport = 0x13,
            Execute = 0x14,
            RemovePartyMember = 0x15,
            EndDialogue = 0x16,
            Wipe = 0x17,
            PlayAnimation = 0x18,
            Offset = 0x19,
            Pause = 0x1A,
            SimpleChest = 0x1B,
            AskSurrender = 0x1C,
            DoScript = 0x1D
        }

        public int Id { get; set; }
        public EventType Type { get; set; }
        public byte Unk1;
        public byte Unk2;
        public byte Unk3;
        public byte Unk4;
        public byte Unk5;
        public ushort Unk6;
        public ushort Unk8;
        public ushort? NextEventId;

        public static MapEvent Load(BinaryReader br, int id)
        {
            var e = new MapEvent();
            e.Id = id;
            e.Type = (EventType)br.ReadByte(); // +0
            e.Unk1 = br.ReadByte(); // +1
            e.Unk2 = br.ReadByte(); // +2
            e.Unk3 = br.ReadByte(); // +3
            e.Unk4 = br.ReadByte(); // +4
            e.Unk5 = br.ReadByte(); // +5
            e.Unk6 = br.ReadUInt16(); // +6
            e.Unk8 = br.ReadUInt16(); // +8
            e.NextEventId = br.ReadUInt16(); // +A
            if (e.NextEventId == 0xffff) e.NextEventId = null;
            return e;
        }

        public override string ToString() => $"Event{Id}->{NextEventId}: {Type} {Unk1} {Unk2} {Unk3} {Unk4} {Unk5} {Unk6} {Unk8} ";
    }
}