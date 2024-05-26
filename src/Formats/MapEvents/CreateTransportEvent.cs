using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("create_transport")]
public class CreateTransportEvent : MapEvent
{
    public CreateTransportEvent(byte x, byte y, byte id, MapId mapId)
    {
        X = x;
        Y = y;
        Id = id;
        MapId = mapId;
    }

    CreateTransportEvent() { }

    public static CreateTransportEvent Serdes(CreateTransportEvent e, AssetMapping mapping, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new CreateTransportEvent();
        e.X = s.UInt8(nameof(X), e.X);
        e.Y = s.UInt8(nameof(Y), e.Y);
        e.Id = s.UInt8(nameof(Id), e.Id);

        int zeroed = s.UInt8(null, 0);
        zeroed += s.UInt8(null, 0);

        e.MapId = MapId.SerdesU16(nameof(MapId), e.MapId, mapping, s);

        zeroed += s.UInt16(null, 0);
        return e;
    }

    [EventPart("x")] public byte X { get; private set; }
    [EventPart("y")] public byte Y { get; private set; }
    [EventPart("id")] public byte Id { get; private set; } // TODO: What kind of id is this?
    [EventPart("map")] public MapId MapId { get; private set; } // 0 = stay on current map

    public override MapEventType EventType => MapEventType.CreateTransport;
}

