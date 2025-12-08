using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

public enum MapExitType
{
    Normal = 0,
    Teleporter = 1,
    TrapdoorUp = 2,
    Jump = 3,
    EndSequence = 4,
    TrapdoorDown = 5,
    Shuttle = 6,
    None = 255
}

[Event("teleport", "teleports the party to a specific location")]
public class TeleportEvent : MapEvent // aka MapExit event
{
    TeleportEvent() { }
    public TeleportEvent(MapId mapId, byte x, byte y, Direction direction, byte unk4, byte unk5)
    {
        MapId = mapId;
        X = x;
        Y = y;
        Direction = direction;
        Unk4 = unk4;
        Unk5 = unk5;
    }

    public static TeleportEvent Serdes(TeleportEvent e, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new TeleportEvent();
        e.X = s.UInt8(nameof(X), e.X);
        e.Y = s.UInt8(nameof(Y), e.Y);
        e.Direction = s.EnumU8(nameof(Direction), e.Direction);
        e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
        e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
        e.MapId = MapId.SerdesU16(nameof(MapId), e.MapId, mapping, s);
        int zeroed = s.UInt16(null, 0);
        s.Assert(e.Unk4 is 0 or 1 or 2 or 3 or 6 or 106 or 255, "TeleportEvent: Expected field 4 to be in { 0,1,2,3,6,106,255 }"); // Always 255 in maps
        s.Assert(zeroed == 0, "TeleportEvent: Expected field 8 to be 0");
        return e;
    }

    [EventPart("map")] public MapId MapId { get; private set; } // 0 = stay on current map
    [EventPart("x")] public byte X { get; private set; }
    [EventPart("y")] public byte Y { get; private set; }
    [EventPart("dir", true, Direction.Unchanged)] public Direction Direction { get; private set; }
    [EventPart("unk4", true, (byte)255)] public byte Unk4 { get; private set; } // 255 on 2D maps, (1,6) on 3D maps - probably "MapExitType"
    [EventPart("unk5", true, (byte)2)] public byte Unk5 { get; private set; } // 2,3,4,5,6,8,9
    public override MapEventType EventType => MapEventType.MapExit;
}