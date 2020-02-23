using UAlbion.Formats.Parsers;

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

        public static SetMapLightingEvent Translate(SetMapLightingEvent e, ISerializer s)
        {
            e ??= new SetMapLightingEvent();
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.LightLevel = s.EnumU16(nameof(LightLevel), e.LightLevel);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }

        public byte Unk2 { get; private set; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public LightingLevel LightLevel { get; private set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"set_map_lighting {LightLevel} ({Unk2} {Unk3} {Unk4} {Unk5} {Unk8})";
        public override ModifyType SubType => ModifyType.SetMapLighting;
    }
}
