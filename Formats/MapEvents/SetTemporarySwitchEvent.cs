using System.Diagnostics;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class SetTemporarySwitchEvent : ModifyEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type, ModifyType subType)
        {
            var e = new SetTemporarySwitchEvent
            {
                SwitchValue = br.ReadByte(), // 2
                Unk3 = br.ReadByte(), // 3
                Unk4 = br.ReadByte(), // 4
                Unk5 = br.ReadByte(), // 5
                SwitchId = br.ReadUInt16(), // 6
                Unk8 = br.ReadUInt16(), // 8
            };
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk8 == 0);
            return new EventNode(id, e);
        }

        public byte SwitchValue { get; private set; } // 0,1,2
        public ushort SwitchId { get; private set; } // [0..599]
        public byte Unk3 { get; set; } // 0,1,21
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"set_temporary_switch {SwitchId} {SwitchValue} ({Unk3})";
    }
}
