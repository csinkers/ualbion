using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("change_npc_movement", "Modify an NPC's movement mode")]
public class ChangeNpcMovementEvent(
    byte npcNum,
    NpcMovement mode,
    EventScope scope,
    ChangeIconLayers layers,
    MapId mapId,
    short y)
    : MapEvent, INpcEvent // Specialised variant of ChangeIconEvent
{
    public static ChangeNpcMovementEvent Serdes(ChangeNpcMovementEvent e, AssetMapping mapping, MapType mapType, ISerializer s)
    {
        if (s.IsReading()) // Should never be used
            return (ChangeNpcMovementEvent)ChangeIconEvent.Serdes(null, mapping, mapType, s);

        ArgumentNullException.ThrowIfNull(e);
        var cie = new ChangeIconEvent(e.NpcNum, 0, e.Scope, IconChangeType.NpcMovement, (ushort)e.Mode, e.Layers, e.MapId);
        ChangeIconEvent.Serdes(cie, mapping, mapType, s);
        return e;
    }

    public static ChangeNpcMovementEvent FromChangeIconEvent(ChangeIconEvent cie)
    {
        ArgumentNullException.ThrowIfNull(cie);

        if (cie.X is < 0 or > byte.MaxValue) throw new FormatException($"Expected X to be in range [0..{byte.MaxValue}] for ChangeNpcMovementEvent but was {cie.X}");
        if (cie.ChangeType != IconChangeType.NpcMovement) throw new FormatException($"Expected ChangeType to be NpcMovement for ChangeNpcMovementEvent, but was {cie.ChangeType}");
        if (cie.Scope is EventScope.RelPerm or EventScope.RelTemp) throw new FormatException($"Expected Scope to be absolute for ChangeNpcMovementEvent, but was {cie.Scope}");

        return new ChangeNpcMovementEvent((byte)cie.X, (NpcMovement)cie.Value, cie.Scope, cie.Layers, cie.MapId, cie.Y);
    }

    [EventPart("npc")] public byte NpcNum { get; } = npcNum;
    [EventPart("mode")] public NpcMovement Mode { get; } = mode;
    [EventPart("scope")] public EventScope Scope { get; } = scope;
    [EventPart("layers", true, (ChangeIconLayers)3)] public ChangeIconLayers Layers { get; } = layers; // Only applies to the block change types
    [EventPart("mapId", true, "None")] public MapId MapId { get; } = mapId; // None = current map
    [EventPart("y", true, (short)0)] public short Y { get; } = y;
    public override MapEventType EventType => MapEventType.ChangeIcon;
}