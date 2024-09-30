using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("change_npc_sprite", "Modify an NPC's sprite or 3D sprite group")]
public class ChangeNpcSpriteEvent(byte npcNum, AssetId id, EventScope scope, ChangeIconLayers layers, MapId mapId)
    : MapEvent, INpcEvent // Specialised variant of ChangeIconEvent
{
    public static ChangeNpcSpriteEvent Serdes(ChangeNpcSpriteEvent e, AssetMapping mapping, MapType mapType, ISerializer s)
    {
        if (s.IsReading()) // Should never be used
            return (ChangeNpcSpriteEvent)ChangeIconEvent.Serdes(null, mapping, mapType, s);

        ArgumentNullException.ThrowIfNull(e);
        var value = e.SpriteOrGroup.ToDisk(mapping);
        if (value is < 0 or > ushort.MaxValue)
            throw new FormatException($"Sprite/group id {e.SpriteOrGroup} mapped to disk id {value}, which is outside the allowed range of 0-{ushort.MaxValue}");

        var cie = new ChangeIconEvent(e.NpcNum, 0, e.Scope, IconChangeType.NpcSprite, (ushort)value, e.Layers, e.MapId);
        ChangeIconEvent.Serdes(cie, mapping, mapType, s);
        return e;
    }

    public static ChangeNpcSpriteEvent FromChangeIconEvent(ChangeIconEvent cie, AssetMapping mapping, MapType mapType)
    {
        ArgumentNullException.ThrowIfNull(cie);
        if (cie.X is < 0 or > byte.MaxValue) throw new FormatException($"Expected X to be in range [0..{byte.MaxValue}] for ChangeNpcSpriteEvent but was {cie.X}");
        if (cie.Y != 0) throw new FormatException($"Expected Y to be 0 for ChangeNpcSpriteEvent but was {cie.Y}");
        if (cie.ChangeType != IconChangeType.NpcSprite) throw new FormatException($"Expected ChangeType to be NpcSprite for ChangeNpcSpriteEvent, but was {cie.ChangeType}");

        AssetId spriteOrGroup =
            mapType switch
            {
                MapType.Unknown => new AssetId(AssetType.Unknown, cie.Value),
                MapType.ThreeD => new AssetId(AssetType.ObjectGroup, cie.Value),
                MapType.TwoD => SpriteId.FromDisk(AssetType.NpcLargeGfx, cie.Value, mapping),
                MapType.TwoDOutdoors => SpriteId.FromDisk(AssetType.NpcSmallGfx, cie.Value, mapping),
                _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
            };

        return new ChangeNpcSpriteEvent((byte)cie.X, spriteOrGroup, cie.Scope, cie.Layers, cie.MapId);
    }

    [EventPart("npc")] public byte NpcNum { get; } = npcNum;
    [EventPart("id")] public AssetId SpriteOrGroup { get; } = id;
    [EventPart("scope")] public EventScope Scope { get; } = scope;
    [EventPart("layers", true, (ChangeIconLayers)3)] public ChangeIconLayers Layers { get; } = layers; // Only applies to the block change types
    [EventPart("mapId", true, "None")] public MapId MapId { get; } = mapId; // None = current map
    public override MapEventType EventType => MapEventType.ChangeIcon;
}