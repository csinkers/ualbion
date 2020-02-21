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

        public static ActionEvent Translate(ActionEvent e, ISerializer s)
        {
            e ??= new ActionEvent();
            s.EnumU8(nameof(ActionType),
                () => e.SubType,
                x => e.SubType = x,
                x => ((byte)x,x.ToString()));

            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(Unk6));
            s.Dynamic(e, nameof(Unk8));

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
