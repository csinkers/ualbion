using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    public class SetTemporarySwitchEvent : ModifyEvent
    {
        public static SetTemporarySwitchEvent Serdes(SetTemporarySwitchEvent e, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new SetTemporarySwitchEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.SwitchId = SwitchId.SerdesU16(nameof(SwitchId), e.SwitchId, mapping, s);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            ApiUtil.Assert(e.Unk4 == 0);
            ApiUtil.Assert(e.Unk5 == 0);
            ApiUtil.Assert(e.Unk8 == 0);
            return e;
        }

        public enum SwitchOperation : byte
        {
            Reset,
            Set,
            Toggle
        }

        public SwitchOperation Operation { get; private set; } // 0,1,2
        public SwitchId SwitchId { get; private set; } // [0..599]
        public byte Unk3 { get; private set; } // 0,1,21
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"set_temporary_switch {SwitchId} {Operation} ({Unk3})";
        public override ModifyType SubType => ModifyType.SetTemporarySwitch;
    }
}
