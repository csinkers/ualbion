using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public static class NpcMapping
{
    public delegate (int? tileId, int w, int h) GetTileFunc(AssetId id);
    public static class Prop
    {
        public const string Id = "Id";
        public const string Visual = "Visual";
        public const string Flags = "Flags";
        public const string Triggers = "Triggers";
        public const string Movement = "Movement";
        public const string Script = "Script";
        public const string Sound = "Sound";
        public const string Type = "Type";
        public const string Path = "Path";
    }

    public static IEnumerable<ObjectGroup> BuildNpcs(
        BaseMapData map,
        int tileWidth,
        int tileHeight,
        GetTileFunc getTileFunc,
        Dictionary<ushort, string> functionsByEventId,
        ref int nextObjectGroupId,
        ref int nextObjectId)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));

        int nextId = nextObjectId;
        int npcGroupId = nextObjectGroupId++;

        var waypointGroups = new List<ObjectGroup>();
        var npcPathIndices = new Dictionary<int, int>();
        for (var index = 0; index < map.Npcs.Count; index++)
        {
            var npc = map.Npcs[index];
            if (!npc.HasWaypoints(map.Flags))
                continue;

            if (npc.SpriteOrGroup == AssetId.None) // Unused 2D slots
                continue;

            if (npc.SpriteOrGroup == new AssetId(AssetType.ObjectGroup, 1) && npc.Id.IsNone) // unused 3D slots
                continue;

            int firstWaypointObjectId = nextId;
            npcPathIndices[index] = firstWaypointObjectId;
            waypointGroups.Add(new ObjectGroup
            {
                Id = nextObjectGroupId++,
                Name = $"NPC{index} Path",
                Objects = NpcPathBuilder.Build(index, npc.Waypoints, tileWidth, tileHeight, ref nextId),
                Hidden = true,
            });
        }

        var group = new ObjectGroup
        {
            Id = npcGroupId,
            Name = "NPCs",
            Objects = map.Npcs.Select((x, i) =>
                    BuildNpcObject(
                        tileWidth,
                        tileHeight,
                        functionsByEventId,
                        getTileFunc,
                        npcPathIndices,
                        i,
                        x,
                        ref nextId))
                .ToList(),
        };

        nextObjectId = nextId;
        return new[] { group }.Concat(waypointGroups);
    }

    static MapObject BuildNpcObject(
        int tileWidth,
        int tileHeight,
        Dictionary<ushort, string> functionsByEventId,
        GetTileFunc getTileFunc,
        Dictionary<int, int> npcPathIndices,
        int npcIndex,
        MapNpc npc,
        ref int nextId)
    {
        var objProps = new List<TiledProperty>
        {
            new(Prop.Visual, npc.SpriteOrGroup.ToString()),
            new(Prop.Flags, npc.Flags.ToString()),
            new(Prop.Triggers, npc.Triggers.ToString()),
            new(Prop.Movement, npc.Movement.ToString()),
            new(Prop.Type, npc.Type.ToString()),
        };

        var script = npc.Node != null ? functionsByEventId[npc.Node.Id] : null;

        if (!npc.Id.IsNone) objProps.Add(new TiledProperty(Prop.Id, npc.Id.ToString()));
        if (npc.Node != null) objProps.Add(new TiledProperty(Prop.Script, script));
        if (npc.Sound != 0) objProps.Add(new TiledProperty(Prop.Sound, npc.Sound.ToString()));
        if (npcPathIndices.TryGetValue(npcIndex, out var pathObjectId)) objProps.Add(TiledProperty.Object(Prop.Path, pathObjectId));

        var (tileId, tileW, tileH) = getTileFunc(npc.SpriteOrGroup);
        return new MapObject
        {
            Id = nextId++,
            Gid = tileId ?? 0,
            Name = $"NPC{npcIndex} {npc.Id} {script}",
            Type = ObjectGroupMapping.TypeName.Npc,
            X = npc.Waypoints[0].X * tileWidth,
            Y = npc.Waypoints[0].Y * tileHeight,
            Width = tileW,
            Height = tileH,
            Properties = objProps
        };
    }

    public static MapNpc ParseNpc(MapObject obj, int tileWidth, int tileHeight, Func<string, ushort> resolveEntryPoint, NpcPathParser pathParser)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        if (resolveEntryPoint == null) throw new ArgumentNullException(nameof(resolveEntryPoint));
        if (pathParser == null) throw new ArgumentNullException(nameof(pathParser));

        var position = ((int)obj.X / tileWidth, (int)obj.Y / tileHeight);
        NpcWaypoint[] waypoints = { new((byte)position.Item1, (byte)position.Item2) };

        var id = obj.PropString(Prop.Id);
        var visual = obj.PropString(Prop.Visual);
        if (string.IsNullOrEmpty(visual))
            throw new FormatException(
                $"NPC \"{obj.Name}\" (id {obj.Id}) requires a Visual property to determine its appearance");

        var entryPointName = obj.PropString(Prop.Script);
        var entryPoint = resolveEntryPoint(entryPointName);

        var pathStart = obj.PropInt(Prop.Path);
        if (pathStart.HasValue)
            waypoints = pathParser.GetWaypoints(pathStart.Value, MapNpc.WaypointCount);

        return new MapNpc
        {
            Id = string.IsNullOrEmpty(id) ? AssetId.None : AssetId.Parse(id),
            Node = entryPoint == EventNode.UnusedEventId ? null : new DummyEventNode(entryPoint),
            Waypoints = waypoints,
            Type = (NpcType)Enum.Parse(typeof(NpcType), obj.PropString(Prop.Type)),
            Flags = (MapNpcFlags)Enum.Parse(typeof(MapNpcFlags), obj.PropString(Prop.Flags)),
            Triggers = (TriggerTypes)Enum.Parse(typeof(TriggerTypes), obj.PropString(Prop.Triggers)),
            Movement = (NpcMovement)Enum.Parse(typeof(NpcMovement), obj.PropString(Prop.Movement)),
            Sound = (byte)(obj.PropInt(Prop.Sound) ?? 0),
            SpriteOrGroup = AssetId.Parse(visual) // TODO: Handle groups for 3D maps
        };
    }
}
