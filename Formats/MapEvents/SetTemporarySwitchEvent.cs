using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class SetTemporarySwitchEvent : ModifyEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type, ModifyType subType)
        {
            return new EventNode(id, new SetTemporarySwitchEvent
            {
                SwitchValue = br.ReadByte(), // 2
                Unk3 = br.ReadByte(), // 3
                Unk4 = br.ReadByte(), // 4
                Unk5 = br.ReadByte(), // 5
                SwitchId = br.ReadUInt16(), // 6
                Unk8 = br.ReadUInt16(), // 8
            });
        }

        public byte SwitchValue { get; private set; }
        public ushort SwitchId { get; private set; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"set_temporary_switch {SwitchId} {SwitchValue} ({Unk3} {Unk4} {Unk5} {Unk8})";
    }
}
