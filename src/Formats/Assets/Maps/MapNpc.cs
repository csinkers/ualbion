﻿using System;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets.Maps;

public class MapNpc // 0xA = 10 bytes
{
    public const int SizeOnDisk = 10;
    public const int WaypointCount = 0x480;
    public static MapNpc Unused => new() { Waypoints = [new NpcWaypoint(0, 0)] };

    MapNpcFlags _raw;
    public AssetId Id { get; set; } // MonsterGroup, Npc etc
    public byte Sound { get; set; }
    public AssetId SpriteOrGroup { get; set; } // LargeNpcGfx, SmallNpcGfx etc but could also be an ObjectGroup for 3D
    public bool IsUnused => Id == AssetId.None && SpriteOrGroup == AssetId.None;

    public NpcType Type
    {
        get => (NpcType)(
            ((_raw & MapNpcFlags.Type1) != 0 ? 1 : 0) |
            ((_raw & MapNpcFlags.Type2) != 0 ? 2 : 0));
        set => _raw =
            _raw & ~MapNpcFlags.TypeMaskV2
            | (((int)value & 1) != 0 ? MapNpcFlags.Type1 : 0)
            | (((int)value & 2) != 0 ? MapNpcFlags.Type2 : 0)
            | (((int)value & 4) != 0 ? MapNpcFlags.Type4 : 0);
    }

    public MapNpcFlags Flags
    {
        get => _raw & MapNpcFlags.MiscMaskV2;
        set => _raw = _raw & ~MapNpcFlags.MiscMaskV2 | (value & MapNpcFlags.MiscMaskV2);
    }

    public NpcMovement Movement
    {
        get => (NpcMovement)(
            ((_raw & MapNpcFlags.MoveB1) != 0 ? 1 : 0) |
            ((_raw & MapNpcFlags.MoveB2) != 0 ? 2 : 0) |
            ((_raw & MapNpcFlags.MoveB4) != 0 ? 4 : 0) |
            ((_raw & MapNpcFlags.MoveB8) != 0 ? 8 : 0));
        set => _raw =
            _raw & ~MapNpcFlags.MoveMaskV2
            | (((int)value & 1) != 0 ? MapNpcFlags.MoveB1 : 0)
            | (((int)value & 2) != 0 ? MapNpcFlags.MoveB2 : 0)
            | (((int)value & 4) != 0 ? MapNpcFlags.MoveB4 : 0)
            | (((int)value & 8) != 0 ? MapNpcFlags.MoveB8 : 0);
    }

    public TriggerTypes Triggers { get; set; }
    public NpcWaypoint[] Waypoints { get; set; }
    public ushort Chain { get; set; }
    [JsonIgnore] public IEventNode Node { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public ushort EventIndex
    {
        get => Node?.Id ?? 0xffff;
        set => Node = value == 0xffff ? null : new DummyEventNode(value);
    }

    public bool HasWaypoints(MapFlags mapFlags) => Movement is NpcMovement.Waypoints or NpcMovement.Waypoints2;

    public static MapNpc Serdes(SerdesName _, MapNpc existing, MapType mapType, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        s.Begin("Npc");
        var offset = s.Offset;
        var npc = existing ?? new MapNpc();

        byte id = (byte)npc.Id.ToDisk(mapping);
        id = s.UInt8(nameof(Id), id);
        npc.Sound = s.UInt8(nameof(Sound), npc.Sound);

        ushort rawEventNumber = npc.Node?.Id ?? 0xffff;
        rawEventNumber = s.UInt16(nameof(npc.Node), rawEventNumber);
        ushort? eventNumber = rawEventNumber == 0xffff ? null : rawEventNumber;

        if (eventNumber != null && npc.Node == null)
            npc.Node = new DummyEventNode(eventNumber.Value);

        // TODO: Use Large/SmallPartyGfx when type is Party
        npc.SpriteOrGroup = mapType switch
        {
            MapType.ThreeD => AssetId.SerdesU16(nameof(SpriteOrGroup), npc.SpriteOrGroup, AssetType.ObjectGroup, mapping, s),
            MapType.TwoD => SpriteId.SerdesU16(nameof(SpriteOrGroup), npc.SpriteOrGroup, AssetType.NpcLargeGfx, mapping, s),
            MapType.TwoDOutdoors => SpriteId.SerdesU16(nameof(SpriteOrGroup), npc.SpriteOrGroup, AssetType.NpcSmallGfx, mapping, s),
            _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
        };

        npc._raw = s.EnumU16(nameof(Flags), npc._raw);
        npc.Triggers = s.EnumU16(nameof(Triggers), npc.Triggers);

        var assetType = AssetTypeForNpcType(npc.Type, (npc.Flags & MapNpcFlags.SimpleMsg) != 0);
        npc.Id = AssetId.FromDisk(assetType, id, mapping);

        s.End();
        var actualSize = s.Offset - offset;
        if (actualSize != 10)
            throw new FormatException("NPC was not 10 bytes!");
        return npc;
    }

    public static AssetType AssetTypeForNpcType(NpcType type, bool simpleMessageFlag)
    {
        if (simpleMessageFlag)
            return AssetType.MapTextIndex;

        return type switch
        {
            NpcType.Party => AssetType.EventSet, // TODO: Add the 980 offset
            NpcType.Monster => AssetType.MonsterGroup,
            NpcType.Prop => AssetType.MonsterGroup,
            _ => AssetType.NpcSheet // NPCs
        };

    }
    public void LoadWaypoints(ISerdes s, bool useWaypoints)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (useWaypoints)
        {
            Waypoints ??= new NpcWaypoint[WaypointCount];
            for (int i = 0; i < Waypoints.Length; i++)
            {
                byte x = s.UInt8("x", Waypoints[i].X);
                byte y = s.UInt8("y", Waypoints[i].Y);
                Waypoints[i] = new NpcWaypoint(x, y);
            }
        }
        else
        {
            var wp = Waypoints?[0];
            byte x = wp?.X ?? 0;
            byte y = wp?.Y ?? 0;
            x = s.UInt8("X", x);
            y = s.UInt8("Y", y);
            Waypoints = [new NpcWaypoint(x, y)];
        }
    }

    public void Unswizzle(MapId mapId, Func<ushort, IEventNode> getEvent, Func<ushort, ushort> getChain)
    {
        ArgumentNullException.ThrowIfNull(getEvent);
        ArgumentNullException.ThrowIfNull(getChain);

        if (Node is DummyEventNode dummy)
        {
            Node = getEvent(dummy.Id);
            Chain = getChain(dummy.Id);
        }
        else
        {
            Chain = 0xffff;
        }
    }

    public static int WaypointIndexToTime(int index)
    {
        var hours = index / 48;
        var minutes = index % 48;
        return hours * 100 + minutes;
    }

    public static int TimeToWaypointIndex(int time)
    {
        var hours = time / 100;
        var minutes = time % 100;
        if (hours < 0) throw new FormatException($"Time {time} had a negative hours component");
        // Allow 2400 as it's used for the fake final position when parsing
        if (hours == 24 && minutes > 0 || hours > 24) throw new FormatException($"Time {time} had an hours component greater than the maximum (23)");
        if (minutes > 47) throw new FormatException($"Time {time} had a minutes component greater than the maximum (47)");
        return hours * 48 + minutes;
    }

    public override string ToString() => $"Npc{Id.Id} {Id} O:{SpriteOrGroup} F:{Flags:x} M{Movement} S{Sound} E{Node?.Id}";
}
