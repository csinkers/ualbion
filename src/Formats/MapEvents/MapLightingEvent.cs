using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("map_lighting")]
public class MapLightingEvent : ModifyEvent
{
    MapLightingEvent() { }
    public MapLightingEvent(LightingLevel level, byte unk3)
    {
        LightLevel = level;
        Unk3 = unk3;
    }

    public static MapLightingEvent Serdes(MapLightingEvent e, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new MapLightingEvent();
        int temp = s.UInt8("Operation", 3);
        if (temp != 3)
            s.Assert(false, $"MapLightingEvent: Expected operation to be SetAmount (3), but it was {(NumericOperation)temp} ({temp})");

        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
        if (e.Unk3 != 0 && e.Unk3 != 1)
            s.Assert(false, $"MapLightingEvent: Expected field 3 to be 0 or 1, but it was {e.Unk3}");

        temp = s.UInt8("b4", 0);
        if (temp != 0)
            s.Assert(false, $"MapLightingEvent: Expected field 3 to be 0, but it was {temp}");

        temp = s.UInt8("b5", 0);
        if (temp != 0)
            s.Assert(false, $"MapLightingEvent: Expected field 3 to be 0, but it was {temp}");

        e.LightLevel = s.EnumU16(nameof(LightLevel), e.LightLevel);

        temp = s.UInt16("w8", 0);
        if (temp != 0)
            s.Assert(false, $"MapLightingEvent: Expected field 3 to be 0, but it was {temp}");

        return e;
    }

    [EventPart("level")] public LightingLevel LightLevel { get; private set; }
    [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; }
    public override ModifyType SubType => ModifyType.MapLighting;
}