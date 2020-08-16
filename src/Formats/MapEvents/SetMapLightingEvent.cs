using System;
using SerdesNet;

namespace UAlbion.Formats.MapEvents
{
    public class SetMapLightingEvent : ModifyEvent
    {
        public enum LightingLevel : ushort
        {
            Normal = 0,
            NeedTorch = 1,
            FadeFromBlack = 2
        }

        public static SetMapLightingEvent Serdes(SetMapLightingEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new SetMapLightingEvent();
            s.Begin();
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.LightLevel = s.EnumU16(nameof(LightLevel), e.LightLevel);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            s.End();
            return e;
        }

        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; }
        public LightingLevel LightLevel { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"set_map_lighting {LightLevel} ({Unk2} {Unk3} {Unk4} {Unk5} {Unk8})";
        public override ModifyType SubType => ModifyType.SetMapLighting;
    }
}
