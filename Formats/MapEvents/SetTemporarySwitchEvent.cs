using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class SetTemporarySwitchEvent : ModifyEvent
    {
        public static SetTemporarySwitchEvent Translate(SetTemporarySwitchEvent e, ISerializer s)
        {
            e ??= new SetTemporarySwitchEvent();
            s.Dynamic(e, nameof(SwitchValue));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(SwitchId));
            s.Dynamic(e, nameof(Unk8));
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
