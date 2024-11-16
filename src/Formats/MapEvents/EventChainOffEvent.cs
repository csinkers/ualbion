using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("chain_off")]
public class EventChainOffEvent : ModifyEvent
{
    EventChainOffEvent() { }

    public EventChainOffEvent(SwitchOperation operation, byte chainNumber, MapId map)
    {
        Operation = operation;
        ChainNumber = chainNumber;
        Map = map;
    }

    public static EventChainOffEvent Serdes(EventChainOffEvent e, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new EventChainOffEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.ChainNumber = s.UInt8(nameof(ChainNumber), e.ChainNumber);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.Map = MapId.SerdesU16(nameof(Map), e.Map, mapping, s);
        zeroes += s.UInt16(null, 0);
        s.Assert(zeroes == 0, "EventChainOff: fields 4,5,8 are expected to be 0");

        return e;
    }

    [EventPart("op")] public SwitchOperation Operation { get; private set; }
    [EventPart("chain")] public byte ChainNumber { get; private set; }
    [EventPart("map", true, "None")] public MapId Map { get; private set; }
    public override ModifyType SubType => ModifyType.EventChainOff;
}