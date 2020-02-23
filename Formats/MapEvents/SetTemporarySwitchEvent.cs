using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class SetTemporarySwitchEvent : ModifyEvent
    {
        public static SetTemporarySwitchEvent Translate(SetTemporarySwitchEvent e, ISerializer s)
        {
            e ??= new SetTemporarySwitchEvent();
            e.SwitchValue = s.UInt8(nameof(SwitchValue), e.SwitchValue);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.SwitchId = s.UInt16(nameof(SwitchId), e.SwitchId);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk8 == 0);
            return e;
        }

        public byte SwitchValue { get; private set; } // 0,1,2
        public ushort SwitchId { get; private set; } // [0..599]
        public byte Unk3 { get; set; } // 0,1,21
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"set_temporary_switch {SwitchId} {SwitchValue} ({Unk3})";
        public override ModifyType SubType => ModifyType.SetTemporarySwitch;
    }
}
