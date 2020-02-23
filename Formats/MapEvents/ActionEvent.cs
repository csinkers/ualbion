using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class ActionEvent : IMapEvent
    {
        public enum ActionType : byte
        {
            Word = 0,
            Unk1 = 1,
            Unk2 = 2,
            Unk4 = 4,
            Unk5 = 5,
            StartDialogue = 6,
            Unk7 = 7,
            DialogueLine = 8,
            Unk9 = 9,
            Unk14 = 14,
            Unk23 = 23,
            Unk45 = 45,
            UseItem = 46,
            EquipItem = 47,
            UnequipItem = 48,
            PickupItem = 54,
            Unk57 = 57,
            Unk61 = 61
        }

        public static ActionEvent Serdes(ActionEvent e, ISerializer s)
        {
            e ??= new ActionEvent();
            e.SubType = s.EnumU8(nameof(ActionType), e.SubType);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);

            Debug.Assert(e.Unk2 == 1 || ((int)e.SubType == 14 && e.Unk2 == 2));
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk8 == 0);
            return e;
        }

        public ActionType SubType { get; private set; }
        public byte Unk2 { get; private set; } // Always 1, unless SubType == 14 (in which cas it is 2)
        public byte Unk3 { get; private set; } // BLOK?
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; } // TextId?? 0..1216 + 30,000 & 32,000
        ushort Unk8 { get; set; }
        public override string ToString() => $"action {SubType} {Unk3}: {Unk6} ({Unk2})";
        public MapEventType EventType => MapEventType.Action;
    }
}
