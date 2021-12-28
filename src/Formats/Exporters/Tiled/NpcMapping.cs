using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public static class NpcMapping
{
    public delegate (int? tileId, int w, int h) GetTileFunc(AssetId id);
    public static class NpcPropName
    {
        public const string Id = "Id";
        public const string Visual = "Visual";
        public const string Flags = "Flags";
        public const string Movement = "Movement";
        public const string Unk8 = "Unk8";
        public const string Unk9 = "Unk9";
        public const string Script = "Script";
        public const string Sound = "Sound";
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
        int nextId = nextObjectId;
        int npcGroupId = nextObjectGroupId++;

        var waypointGroups = new List<ObjectGroup>();
        var npcPathIndices = new Dictionary<int, int>();
        foreach (var npc in map.Npcs)
        {
            if ((npc.Movement & NpcMovementTypes.RandomMask) != 0)
                continue;

            if (npc.SpriteOrGroup == AssetId.None) // Unused 2D slots
                continue;

            if (npc.SpriteOrGroup == new AssetId(AssetType.ObjectGroup, 1) && npc.Id.IsNone) // unused 3D slots
                continue;

            int firstWaypointObjectId = nextId;
            npcPathIndices[npc.Index] = firstWaypointObjectId;
            waypointGroups.Add(new ObjectGroup
            {
                Id = nextObjectGroupId++,
                Name = $"NPC{npc.Index} Path",
                Objects = NpcPathBuilder.Build(npc, tileWidth, tileHeight, ref nextId),
                Hidden = true,
            });
        }

        var group = new ObjectGroup
        {
            Id = npcGroupId,
            Name = "NPCs",
            Objects = map.Npcs.Select(x =>
                    BuildNpcObject(
                        tileWidth,
                        tileHeight,
                        functionsByEventId,
                        getTileFunc,
                        npcPathIndices,
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
        MapNpc npc,
        ref int nextId)
    {
        var objProps = new List<TiledProperty>
        {
            new(NpcPropName.Visual, npc.SpriteOrGroup.ToString()),
            new(NpcPropName.Flags, npc.Flags.ToString()),
            new(NpcPropName.Movement, ((int) npc.Movement).ToString(CultureInfo.InvariantCulture)),
            new(NpcPropName.Unk8, npc.Unk8.ToString(CultureInfo.InvariantCulture)),
            new(NpcPropName.Unk9, npc.Unk9.ToString(CultureInfo.InvariantCulture))
        };

        if (!npc.Id.IsNone) objProps.Add(new TiledProperty(NpcPropName.Id, npc.Id.ToString()));
        if (npc.Node != null) objProps.Add(new TiledProperty(NpcPropName.Script, functionsByEventId[npc.Node.Id]));
        if (npc.Sound > 0) objProps.Add(new TiledProperty(NpcPropName.Sound, npc.Sound.ToString(CultureInfo.InvariantCulture)));
        if (npcPathIndices.TryGetValue(npc.Index, out var pathObjectId)) objProps.Add(TiledProperty.Object(NpcPropName.Path, pathObjectId));

        var (tileId, tileW, tileH) = getTileFunc(npc.SpriteOrGroup);
        return new MapObject
        {
            Id = nextId++,
            Gid = tileId ?? 0,
            Name = $"NPC{npc.Index} {npc.Id}",
            Type = ObjectGroupMapping.ObjectTypeName.Npc,
            X = npc.Waypoints[0].X * tileWidth,
            Y = npc.Waypoints[0].Y * tileHeight,
            Width = tileW,
            Height = tileH,
            Properties = objProps
        };
    }

    public static MapNpc ParseNpc(MapObject obj, int tileWidth, int tileHeight, Func<string, ushort> resolveEntryPoint, Func<int, NpcWaypoint[]> getWaypoints)
    {
        var position = ((int)obj.X / tileWidth, (int)obj.Y / tileHeight);
        NpcWaypoint[] waypoints = { new((byte)position.Item1, (byte)position.Item2) };

        // string RequiredProp(string name) => Prop(name) ?? throw new FormatException($"Required property \"{name}\" was not present on NPC \"{obj.Name}\" (id {obj.Id})");

        var id = obj.PropString(NpcPropName.Id);
        var visual = obj.PropString(NpcPropName.Visual);
        if (string.IsNullOrEmpty(visual))
            throw new FormatException($"NPC \"{obj.Name}\" (id {obj.Id}) requires a Visual property to determine its appearance");

        var entryPointName = obj.PropString(NpcPropName.Script);
        var entryPoint = resolveEntryPoint(entryPointName);

        var pathStart = obj.PropInt(NpcPropName.Path);
        if (pathStart.HasValue)
            waypoints = getWaypoints(pathStart.Value);

        return new MapNpc
        {
            Id = string.IsNullOrEmpty(id) ? AssetId.None : AssetId.Parse(id),
            Node = entryPoint == EventNode.UnusedEventId ? null : new DummyEventNode(entryPoint),
            Waypoints = waypoints,
            Flags = (NpcFlags)Enum.Parse(typeof(NpcFlags), obj.PropString(NpcPropName.Flags)),
            Movement = (NpcMovementTypes)Enum.Parse(typeof(NpcMovementTypes), obj.PropString(NpcPropName.Movement)),
            Unk8 = (byte)(obj.PropInt(NpcPropName.Unk8) ?? 0),
            Unk9 = (byte)(obj.PropInt(NpcPropName.Unk9) ?? 0),
            SpriteOrGroup = AssetId.Parse(visual) // TODO: Handle groups for 3D maps
        };
    }
}