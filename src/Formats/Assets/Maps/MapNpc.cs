using System;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Maps;

public class MapNpc // 0xA = 10 bytes
{
    public const int SizeOnDisk = 10;
    public const int WaypointCount = 0x480;

    NpcFlags _raw;
    public AssetId Id { get; set; } // MonsterGroup, Npc etc
    // public SampleId? Sound { get; set; }
    public byte Sound { get; set; }
    public AssetId SpriteOrGroup { get; set; } // LargeNpcGfx, SmallNpcGfx etc but could also be an ObjectGroup for 3D

    public NpcType Type
    {
        get => (NpcType)(
            ((_raw & NpcFlags.Type1) != 0 ? 1 : 0) |
            ((_raw & NpcFlags.Type2) != 0 ? 2 : 0));
        set => _raw =
            _raw & ~NpcFlags.TypeMask
            | (((int)value &  1) != 0 ? NpcFlags.Type1 : 0)
            | (((int)value &  2) != 0 ? NpcFlags.Type2 : 0);
    } // 1=Dialogue, 2=AutoAttack, 11=ReturnMsg

    public NpcFlags Flags
    {
        get => _raw & NpcFlags.MiscMask;
        set => _raw = _raw & ~NpcFlags.MiscMask | (value & NpcFlags.MiscMask);
    }

    public NpcMoveA MovementA
    {
        get => (NpcMoveA)(
            ((_raw & NpcFlags.MoveA1) != 0 ? 1 : 0) |
            ((_raw & NpcFlags.MoveA2) != 0 ? 2 : 0));
        set => _raw =
            _raw & ~NpcFlags.MoveAMask
            | (((int)value & 1) != 0 ? NpcFlags.MoveA1 : 0)
            | (((int)value & 2) != 0 ? NpcFlags.MoveA2 : 0);

    }

    public NpcMoveB MovementB
    {
        get => (NpcMoveB)(
            ((_raw & NpcFlags.MoveB1) != 0 ? 1 : 0) |
            ((_raw & NpcFlags.MoveB2) != 0 ? 2 : 0) |
            ((_raw & NpcFlags.MoveB4) != 0 ? 4 : 0) |
            ((_raw & NpcFlags.MoveB8) != 0 ? 8 : 0));
        set => _raw =
            _raw & ~NpcFlags.MoveBMask
            | (((int)value & 1) != 0 ? NpcFlags.MoveB1 : 0)
            | (((int)value & 2) != 0 ? NpcFlags.MoveB2 : 0)
            | (((int)value & 4) != 0 ? NpcFlags.MoveB4 : 0)
            | (((int)value & 8) != 0 ? NpcFlags.MoveB8 : 0);
    }

    public NpcWaypoint[] Waypoints { get; set; }
    public MapId ChainSource { get; set; }
    public ushort Chain { get; set; }
    [JsonIgnore] public IEventNode Node { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)] public ushort EventIndex
    {
        get => Node?.Id ?? 0xffff;
        set => Node = value == 0xffff ? null : new DummyEventNode(value);
    }

    public bool HasWaypoints(MapFlags mapFlags) =>
        (mapFlags & MapFlags.NpcMovementMode) != 0
            ? MovementB is NpcMoveB.Waypoints or NpcMoveB.Waypoints2
            : MovementA == NpcMoveA.FollowWaypoints;

    public static MapNpc Serdes(int _, MapNpc existing, MapType mapType, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        s.Begin("Npc");
        var offset = s.Offset;
        var npc = existing ?? new MapNpc();

        byte id = (byte)npc.Id.ToDisk(mapping);
        id = s.UInt8(nameof(Id), id);
        npc.Sound = s.UInt8(nameof(Sound), npc.Sound); // SampleId?

        ushort? eventNumber = MaxToNullConverter.Serdes(nameof(npc.Node), npc.Node?.Id, s.UInt16);
        if (eventNumber != null && npc.Node == null)
            npc.Node = new DummyEventNode(eventNumber.Value);

        // TODO: Use Large/SmallPartyGfx when type is Party
        npc.SpriteOrGroup = mapType switch
        {
            MapType.ThreeD => AssetId.SerdesU16(nameof(SpriteOrGroup), npc.SpriteOrGroup, AssetType.ObjectGroup, mapping, s),
            MapType.TwoD => SpriteId.SerdesU16(nameof(SpriteOrGroup), npc.SpriteOrGroup, AssetType.LargeNpcGraphics, mapping, s),
            MapType.TwoDOutdoors => SpriteId.SerdesU16(nameof(SpriteOrGroup), npc.SpriteOrGroup, AssetType.SmallNpcGraphics, mapping, s),
            _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
        };

        npc._raw = s.EnumU32(nameof(Flags), npc._raw);
        var assetType = AssetTypeForNpcType(npc.Type);
        npc.Id = AssetId.FromDisk(assetType, id, mapping);

        s.End();
        var actualSize = s.Offset - offset;
        if (actualSize != 10)
            throw new FormatException("NPC was not 10 bytes!");
        return npc;
    }

    public static AssetType AssetTypeForNpcType(NpcType type) =>
        type switch
        {
            NpcType.Party => AssetType.EventSet, // TODO: Add the 980 offset
            NpcType.Monster1 => AssetType.MonsterGroup,
            NpcType.Monster2 => AssetType.MonsterGroup,
            _ => AssetType.EventSet
        };

    public void LoadWaypoints(ISerializer s, bool useWaypoints)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
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
            Waypoints = new[] { new NpcWaypoint(x, y) };
        }
    }

    public void Unswizzle(MapId mapId, Func<ushort, IEventNode> getEvent, Func<ushort, ushort> getChain)
    {
        if (getEvent == null) throw new ArgumentNullException(nameof(getEvent));
        if (getChain == null) throw new ArgumentNullException(nameof(getChain));

        ChainSource = mapId;
        if (Node is DummyEventNode dummy)
        {
            Node = getEvent(dummy.Id);
            Chain = getChain(dummy.Id);
        }
        else Chain = 0xffff;
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

    public override string ToString() => $"Npc{Id.Id} {Id} O:{SpriteOrGroup} F:{Flags:x} M{MovementB} S{Sound} E{Node?.Id}";
}