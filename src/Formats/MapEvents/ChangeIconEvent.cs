using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("change_icon", "Modify a map tile's visual or trigger data", "ci")]
public class ChangeIconEvent : MapEvent
{
    // MapEvent as this method handles ChangeIconEvent, ChangeNpcSpriteEvent and ChangeNpcMovementEvent
    public static MapEvent Serdes(MapEvent me, AssetMapping mapping, MapType mapType, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (me is ChangeNpcSpriteEvent cnse)
            return ChangeNpcSpriteEvent.Serdes(cnse, mapping, mapType, s);

        if (me is ChangeNpcMovementEvent cnme)
            return ChangeNpcMovementEvent.Serdes(cnme, mapping, mapType, s);

        var e = me as ChangeIconEvent;
        if (me != null && e == null)
            throw new ArgumentOutOfRangeException($"Unexpected event {me} passed to ChangeIconEvent.Serdes");

        e ??= new ChangeIconEvent();
        e.X = s.Int8(nameof(X), unchecked((sbyte)e.X)); // 1
        e.Y = s.Int8(nameof(Y), unchecked((sbyte)e.Y)); // 2
        e.Scope = s.EnumU8(nameof(Scope), e.Scope); // 3
        e.ChangeType = s.EnumU8(nameof(ChangeType), e.ChangeType); // 4
        e.Layers = s.EnumU8(nameof(Layers), e.Layers); // 5
        e.Value = s.UInt16(nameof(Value), e.Value); // 6
        e.MapId = MapId.SerdesU16(nameof(MapId), e.MapId, mapping, s); // 8

        if (e.ChangeType == IconChangeType.NpcSprite && s.IsReading())
            return ChangeNpcSpriteEvent.FromChangeIconEvent(e, mapping, mapType);

        if (e.ChangeType == IconChangeType.NpcMovement && s.IsReading())
            return ChangeNpcMovementEvent.FromChangeIconEvent(e);

        if (e.Scope is EventScope.AbsPerm or EventScope.AbsTemp)
        {
            e.X = unchecked((byte)e.X);
            e.Y = unchecked((byte)e.Y);
        }

        return e;
    }

    protected ChangeIconEvent() { }
    public ChangeIconEvent(short x, short y, EventScope scope, IconChangeType changeType, ushort value, ChangeIconLayers layers, MapId mapId)
    {
        X = x;
        Y = y;
        Scope = scope;
        ChangeType = changeType;
        Value = value;
        Layers = layers;
        MapId = mapId;
    }

    [EventPart("x")] public short X { get; private set; } // When ChangeType is NpcMovement/Sprite, this is the NpcId
    [EventPart("y")] public short Y { get; private set; }
    [EventPart("scope")] public EventScope Scope { get; private set; }
    [EventPart("type")] public IconChangeType ChangeType { get; private set; }
    // For most types: value = tile id
    // For NpcMovement: 0=NoChange 4=NoChange
    [EventPart("value")] public ushort Value { get; private set; }
    [EventPart("layers", true, (ChangeIconLayers)3)] public ChangeIconLayers Layers { get; private set; } // Only applies to the block change types
    [EventPart("mapId", true, "None")] public MapId MapId { get; set; } // None = current map
    public override MapEventType EventType => MapEventType.ChangeIcon;
}