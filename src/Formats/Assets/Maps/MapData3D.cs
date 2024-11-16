using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets.Maps;

public class MapData3D : BaseMapData
{
    const int AutomapGraphicsSize = 64;
    public override MapType MapType => MapType.ThreeD;
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
    [JsonInclude] public List<AutomapInfo> Automap { get; set; } = [];

    public byte[] BuildWallArray() => Contents.Select(x => (byte)(x >= LabyrinthData.WallOffset ? x - LabyrinthData.WallOffset + 1 : 0)).ToArray();
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
        PaletteId paletteId,
        LabyrinthId labyrinthId,
        int width, int height,
        IList<EventNode> events, IList<ushort> chains,
        IEnumerable<MapNpc> npcs,
        IList<MapEventZone> zones) : base(id, paletteId, width, height, events, chains, npcs, zones)
    {
        LabDataId = labyrinthId;
        Floors = new byte[width * height];
        Ceilings = new byte[width * height];
        Contents = new byte[width * height];
        AutomapGraphics = new byte[AutomapGraphicsSize];
    }

    public MapData3D(MapId id, PaletteId paletteId, LabyrinthId labyrinthId, int width, int height) : base(id, paletteId, width, height)
    {
        LabDataId = labyrinthId;
        Floors = new byte[width * height];
        Ceilings = new byte[width * height];
        Contents = new byte[width * height];
        AutomapGraphics = new byte[AutomapGraphicsSize];
    }

    public static MapData3D Serdes(AssetId id, MapData3D existing, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);

        var map = existing ?? new MapData3D { Id = id };
        map.Flags = s.EnumU16(nameof(Flags), map.Flags); // 0
        _ = s.UInt8("MapType", (byte)map.MapType); // 2

        if (map.Width > byte.MaxValue + OffsetX) throw new InvalidOperationException($"Cannot save a map with a width above {byte.MaxValue + OffsetX} using original game formats");
        if (map.Height > byte.MaxValue + OffsetY) throw new InvalidOperationException($"Cannot save a map with a height above {byte.MaxValue + OffsetY} using original game formats");

        map.SongId = SongId.SerdesU8(nameof(SongId), map.SongId, mapping, s); // 3
        map.Width = s.UInt8(nameof(Width), (byte)(map.Width - OffsetX)) + OffsetX; // 4
        map.Height = s.UInt8(nameof(Height), (byte)(map.Height - OffsetY)) + OffsetY; // 5
        map.LabDataId = LabyrinthId.SerdesU8(nameof(LabDataId), map.LabDataId, mapping, s); // 6
        map.CombatBackgroundId = CombatBackgroundId.SerdesU8(nameof(CombatBackgroundId), map.CombatBackgroundId, mapping, s); // 7
        map.PaletteId = PaletteId.SerdesU8(nameof(PaletteId), map.PaletteId, mapping, s);
        map.AmbientSongId = SongId.SerdesU8(nameof(AmbientSongId), map.AmbientSongId, mapping, s);

        map.Npcs ??= [];
        int npcCount = (map.Flags & MapFlags.ExtraNpcs) != 0 ? 96 : 32;
        while (map.Npcs.Count < npcCount)
            map.Npcs.Add(new MapNpc());

        map.Npcs = s.List(
            nameof(Npcs),
            map.Npcs,
            npcCount,
            (n, x, s2) => MapNpc.Serdes(n, x, map.MapType, mapping, s2)).ToList();

        s.Begin("TileData");
        map.Contents ??= new byte[map.Width * map.Height];
        map.Floors   ??= new byte[map.Width * map.Height];
        map.Ceilings ??= new byte[map.Width * map.Height];

        int diskWidth = map.Width - OffsetX;
        for (int i = 0; i < (map.Width - OffsetX) * (map.Height - OffsetY); i++)
        {
            int diskX = i % diskWidth;
            int diskY = i / diskWidth;
            int memX = diskX + OffsetX;
            int memY = diskY + OffsetY;
            int memIndex = memY * map.Width + memX;

            map.Contents[memIndex] = s.UInt8(null, map.Contents[memIndex]);
            map.Floors[memIndex]   = s.UInt8(null, map.Floors[memIndex]);
            map.Ceilings[memIndex] = s.UInt8(null, map.Ceilings[memIndex]);
        }
        s.End();

        var zoneCount = map.SerdesZones(s);

        if (s.IsReading() && s.BytesRemaining == 0 || s.IsWriting() && map.AutomapGraphics == null)
        {
            ApiUtil.Assert(zoneCount == 0, "Trivial test map was expected to have 0 zones");
            foreach (var npc in map.Npcs)
                npc.Waypoints = new NpcWaypoint[1];
            return map;
        }

        map.SerdesEvents(mapping, map.MapType, s);
        map.SerdesNpcWaypoints(s);
        map.SerdesAutomap(s);
        map.SerdesChains(s, 64);

        if (s.IsReading())
            map.Unswizzle();

        return map;
    }

    void SerdesAutomap(ISerdes s)
    {
        ushort automapInfoCount = s.UInt16("AutomapInfoCount", (ushort)Automap.Count);
        if (automapInfoCount != 0xffff)
            s.List(nameof(Automap), Automap, automapInfoCount, AutomapInfo.Serdes);

        AutomapGraphics = s.Bytes(nameof(AutomapGraphics), AutomapGraphics, AutomapGraphicsSize);
    }
}