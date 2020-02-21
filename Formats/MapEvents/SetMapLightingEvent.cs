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
            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.EnumU16(nameof(LightLevel),
                () => e.LightLevel,
                x => e.LightLevel = x,
                x => ((ushort)x, x.ToString()));
            s.Dynamic(e, nameof(Unk8));
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
