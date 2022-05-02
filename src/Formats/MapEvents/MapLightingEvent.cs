using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("map_lighting")]
public class MapLightingEvent : ModifyEvent
{
    MapLightingEvent() { }
    public MapLightingEvent(NumericOperation operation, LightingLevel level)
    {
        Operation = operation;
        LightLevel = level;
    }

    public static MapLightingEvent Serdes(MapLightingEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new MapLightingEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        int zeroed = s.UInt8("b3", 0);
        zeroed += s.UInt8("b4", 0);
        zeroed += s.UInt8("b5", 0);
        e.LightLevel = s.EnumU16(nameof(LightLevel), e.LightLevel);
        zeroed += s.UInt16("w8", 0);
        s.Assert(zeroed == 0, "SetMapLightingEvent: Expected fields 3,4,5,8 to be 0");
        return e;
    }

    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("level")] public LightingLevel LightLevel { get; private set; }
    public override ModifyType SubType => ModifyType.MapLighting;
}