using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("set_map_lighting")]
    public class SetMapLightingEvent : ModifyEvent
    {
        SetMapLightingEvent() { }
        public SetMapLightingEvent(LightingLevel level, byte unk2, byte unk3)
        {

            LightLevel = level;
            Unk2 = unk2;
            Unk3 = unk3;
        }

        public static SetMapLightingEvent Serdes(SetMapLightingEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new SetMapLightingEvent();
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            int zeroed = s.UInt8(null, 0);
            zeroed += s.UInt8(null, 0);
            e.LightLevel = s.EnumU16(nameof(LightLevel), e.LightLevel);
            zeroed += s.UInt16(null, 0);
            s.Assert(zeroed == 0, "SetMapLightingEvent: Expected fields 4,5,8 to be 0");
            return e;
        }

        [EventPart("level")] public LightingLevel LightLevel { get; private set; }
        [EventPart("unk2", true, (byte)3)] public byte Unk2 { get; private set; }
        [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; }
        public override ModifyType SubType => ModifyType.MapLighting;
    }
}
