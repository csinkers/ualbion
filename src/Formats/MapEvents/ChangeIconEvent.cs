using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("change_icon", "Modify a map tile's visual or trigger data", "ci")]
public class ChangeIconEvent : MapEvent
{
    public static ChangeIconEvent Serdes(ChangeIconEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new ChangeIconEvent();
        e.X          = s.Int8(nameof(X), unchecked((sbyte)e.X));   // 1
        e.Y          = s.Int8(nameof(Y), unchecked((sbyte)e.Y));   // 2
        e.Scope      = s.EnumU8(nameof(Scope), e.Scope);           // 3
        e.ChangeType = s.EnumU8(nameof(ChangeType), e.ChangeType); // 4
        e.Layers     = s.EnumU8(nameof(Layers), e.Layers);         // 5
        e.Value      = s.UInt16(nameof(Value), e.Value);           // 6
        e.MapId      = MapId.SerdesU16(nameof(MapId), e.MapId, mapping, s); // 8

        if (e.Scope is EventScope.AbsPerm or EventScope.AbsTemp)
        {
            e.X = unchecked((byte)e.X);
            e.Y = unchecked((byte)e.Y);
        }

        return e;
    }

    ChangeIconEvent() { }
    public ChangeIconEvent(short x, short y, EventScope scope, IconChangeType changeType, ushort value, ChangeIconLayers layers)
    {
        X = x;
        Y = y;
        Scope = scope;
        ChangeType = changeType;
        Value = value;
        Layers = layers;
    }

    [EventPart("x")] public short X { get; private set; } // When ChangeType is NpcMovement/Sprite, this is the NpcId
    [EventPart("y")] public short Y { get; private set; }
    [EventPart("scope")] public EventScope Scope { get; private set; }
    [EventPart("type")] public IconChangeType ChangeType { get; private set; }
    // For most types: value = tile id
    // For NpcMovement: 0=NoChange 4=NoChange
    [EventPart("value")] public ushort Value { get; private set; }
    [EventPart("layers", true, (ChangeIconLayers)3)] public ChangeIconLayers Layers { get; private set; } // Only applies to the block change types
    [EventPart("mapId", true, "None")] MapId MapId { get; set; }
    public override MapEventType EventType => MapEventType.ChangeIcon;
}