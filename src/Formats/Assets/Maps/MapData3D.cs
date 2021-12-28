using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Formats.Assets.Maps;

public class MapData3D : BaseMapData
{
    public override MapType MapType => MapType.ThreeD;
    [JsonInclude] public Map3DFlags Flags { get; set; }
    [JsonInclude] public LabyrinthId LabDataId { get; set; }
    [JsonInclude] public SongId AmbientSongId { get; set; }

    /// <summary>
    /// These either refer to object-groups, or walls. If the value is below 100, it is an object-group index.
    /// If the value is above 100, then the value is a wall index + 100.
    /// </summary>
    [JsonInclude] public byte[] Contents { get; set; }
    [JsonInclude] public byte[] Floors { get; set; }
    [JsonInclude] public byte[] Ceilings { get; set; }
    [JsonInclude] public byte[] AutomapGraphics { get; set; }
    [JsonInclude] public List<AutomapInfo> Automap { get; set; } = new();

    public byte[] BuildWallArray() => Contents.Select(x => (byte)(x >= LabyrinthData.WallOffset ? x - LabyrinthData.WallOffset : 0)).ToArray();
    public byte[] BuildObjectArray() => Contents.Select(x => x < LabyrinthData.WallOffset ? x : (byte)0).ToArray();
    public byte GetWall(int index)
    {
        if (index < 0 || index > Contents.Length) return 0;
        var contents = Contents[index];
        return (byte)(contents >= LabyrinthData.WallOffset ? contents - LabyrinthData.WallOffset : 0);
    }

    public byte GetObject(int index)
    {
        if (index < 0 || index > Contents.Length) return 0;
        var contents = Contents[index];
        return contents < LabyrinthData.WallOffset ? contents : (byte)0;
    }

    public MapData3D() { } // For JSON
    public MapData3D(MapId id,
        byte width, byte height,
        IList<EventNode> events, IList<ushort> chains,
        IEnumerable<MapNpc> npcs,
        IList<MapEventZone> zones) : base(id, width, height, events, chains, npcs, zones) { }

    public static MapData3D Serdes(AssetInfo info, MapData3D existing, AssetMapping mapping, ISerializer s)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (s == null) throw new ArgumentNullException(nameof(s));

        var map = existing ?? new MapData3D { Id = info.AssetId };
        map.Flags = s.EnumU8(nameof(Flags), map.Flags); // 0
        map.OriginalNpcCount = s.UInt8(nameof(OriginalNpcCount), map.OriginalNpcCount); // 1
        int npcCount = NpcCountTransform.Instance.FromNumeric(map.OriginalNpcCount);
        var _ = s.UInt8("MapType", (byte)map.MapType); // 2

        map.SongId = SongId.SerdesU8(nameof(SongId), map.SongId, mapping, s); // 3
        map.Width = s.UInt8(nameof(Width), map.Width); // 4
        map.Height = s.UInt8(nameof(Height), map.Height); // 5
        map.LabDataId = LabyrinthId.SerdesU8(nameof(LabDataId), map.LabDataId, mapping, s); // 6
        map.CombatBackgroundId = SpriteId.SerdesU8(nameof(CombatBackgroundId), map.CombatBackgroundId, AssetType.CombatBackground, mapping, s); // 7 TODO: Verify this is combat background
        map.PaletteId = PaletteId.SerdesU8(nameof(PaletteId), map.PaletteId, mapping, s);
        map.AmbientSongId = SongId.SerdesU8(nameof(AmbientSongId), map.AmbientSongId, mapping, s);
        map.Npcs = s.List(
            nameof(Npcs),
            map.Npcs,
            npcCount,
            (n, x, s2) => MapNpc.Serdes(n, x, map.MapType, mapping, s2)).ToArray();

        map.Contents ??= new byte[map.Width * map.Height];
        map.Floors   ??= new byte[map.Width * map.Height];
        map.Ceilings ??= new byte[map.Width * map.Height];

        for (int i = 0; i < map.Width * map.Height; i++)
        {
            map.Contents[i] = s.UInt8(null, map.Contents[i]);
            map.Floors[i]   = s.UInt8(null, map.Floors[i]);
            map.Ceilings[i] = s.UInt8(null, map.Ceilings[i]);
        }
        s.Check();

        map.SerdesZones(s);

        if (s.IsReading() && s.IsComplete() || s.IsWriting() && map.AutomapGraphics == null)
        {
            ApiUtil.Assert(map.Zones.Count == 0);
            foreach (var npc in map.Npcs)
                npc.Waypoints = new NpcWaypoint[1];
            return map;
        }

        map.SerdesEvents(mapping, s);
        map.SerdesNpcWaypoints(s);
        map.SerdesAutomap(s);
        map.SerdesChains(s, 64);

        if (s.IsReading())
            map.Unswizzle();

        return map;
    }

    void SerdesAutomap(ISerializer s)
    {
        ushort automapInfoCount = s.UInt16("AutomapInfoCount", (ushort)Automap.Count);
        if (automapInfoCount != 0xffff)
        {
            s.List(nameof(Automap), Automap, automapInfoCount, AutomapInfo.Serdes);
            s.Check();
        }

        AutomapGraphics = s.Bytes(nameof(AutomapGraphics), AutomapGraphics, 0x40);
        s.Check();
    }
}