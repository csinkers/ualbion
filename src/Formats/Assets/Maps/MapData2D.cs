using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;
using UAlbion.Scripting;

namespace UAlbion.Formats.Assets.Maps;

public class MapData2D : BaseMapData
{
    static readonly Base.Tileset[] OutdoorTilesets =
    { // TODO: Pull from config or infer from other data
        Base.Tileset.Outdoors,
        Base.Tileset.Outdoors2,
        Base.Tileset.Desert
    };
    public override MapType MapType => OutdoorTilesets.Any(x => x == TilesetId) ? MapType.TwoDOutdoors : MapType.TwoD;
    [JsonInclude] public byte Sound { get; set; }
    [JsonInclude] public TilesetId TilesetId { get; set; }
    [JsonInclude] public byte FrameRate { get; set; } // If set to 0, used default value (5)
    [JsonIgnore] public MapTile[] Tiles { get; private set; }
    public byte[] JsonTiles
    {
        get => MapTile.ToPacked(Tiles, 1, 0, 0);
        set => Tiles = MapTile.FromPacked(value, 1, 0, 0);
    }

    byte[] RawLayout
    {
        get => MapTile.ToPacked(Tiles, Width, OffsetX, OffsetY);
        set => Tiles = MapTile.FromPacked(value, Width, OffsetX, OffsetY);
    }

    public MapData2D() { } // For JSON
    public MapData2D(MapId id, PaletteId paletteId, TilesetId tilesetId, int width, int height, EventLayout layout, IEnumerable<MapNpc> npcs, IList<MapEventZone> zones)
        : this(
            id,
            paletteId,
            tilesetId,
            width,
            height,
            layout != null ? layout.Events : throw new ArgumentNullException(nameof(layout)),
            layout.Chains,
            npcs,
            zones)
    {
    }

    public MapData2D(MapId id,
        PaletteId paletteId,
        TilesetId tilesetId,
        int width, int height,
        IList<EventNode> events, IList<ushort> chains,
        IEnumerable<MapNpc> npcs,
        IList<MapEventZone> zones) : base(id, paletteId, width, height, events, chains, npcs, zones)
    {
        TilesetId = tilesetId;
        Tiles = new MapTile[width * height];
    }

    public MapData2D(MapId id, PaletteId paletteId, TilesetId tilesetId, byte width, byte height) : base(id, paletteId, width, height)
    {
        TilesetId = tilesetId;
        Tiles = new MapTile[width * height];
    }

    public static MapData2D Serdes(AssetId id, MapData2D existing, AssetMapping mapping, ISerializer s)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        if (s == null) throw new ArgumentNullException(nameof(s));

        var startOffset = s.Offset;
        var map = existing ?? new MapData2D { Id = id };
        map.Flags = s.EnumU16(nameof(Flags), map.Flags); // 0
        _ = s.UInt8("MapType", (byte)MapType.TwoD); // 2 (always Map2D to start with, may shift to outdoors once we assign the tileset)

        map.SongId = SongId.SerdesU8(nameof(SongId), map.SongId, mapping, s); // 3
        if (map.Width > byte.MaxValue + OffsetX) throw new InvalidOperationException($"Cannot save a map with a width above {byte.MaxValue + OffsetX} using original game formats");
        if (map.Height > byte.MaxValue + OffsetY) throw new InvalidOperationException($"Cannot save a map with a height above {byte.MaxValue + OffsetY} using original game formats");

        map.Width = s.UInt8(nameof(Width), (byte)(map.Width - OffsetX)) + OffsetX; // 4
        map.Height = s.UInt8(nameof(Height), (byte)(map.Height - OffsetY)) + OffsetY; // 5
        map.TilesetId = TilesetId.SerdesU8(nameof(TilesetId), map.TilesetId, mapping, s); //6
        map.CombatBackgroundId = SpriteId.SerdesU8(nameof(CombatBackgroundId), map.CombatBackgroundId, AssetType.CombatBackground, mapping, s); // 7
        map.PaletteId = PaletteId.SerdesU8(nameof(PaletteId), map.PaletteId, mapping, s);
        map.FrameRate = s.UInt8(nameof(FrameRate), map.FrameRate); // 9
        int expectedOffset = 10;
        ApiUtil.Assert(s.Offset - startOffset == expectedOffset, $"Map2D: Expected offset after header to be {expectedOffset:x}, but it was {s.Offset - startOffset:x}");

        map.Npcs ??= new List<MapNpc>();

        int npcCount = (map.Flags & MapFlags.ExtraNpcs) != 0 ? 96 : 32;
        while (map.Npcs.Count < npcCount)
            map.Npcs.Add(new MapNpc());

        map.Npcs = s.List(
            nameof(Npcs),
            map.Npcs,
            npcCount,
            (n, x, s2) => MapNpc.Serdes(n, x, map.MapType, mapping, s2)).ToList();

        expectedOffset = 10 + npcCount * MapNpc.SizeOnDisk;
        ApiUtil.Assert(s.Offset - startOffset == expectedOffset, $"Map2D: Expected offset after NPCs to be {expectedOffset:x}, but it was {s.Offset - startOffset:x}");

        int tileCount = (map.Width - OffsetX) * (map.Height - OffsetY);

        if (s.IsReading())
            map.RawLayout = s.Bytes("Layout", null, 3 * tileCount);
        else
            s.Bytes("Layout", map.RawLayout, 3 * tileCount);

        expectedOffset = 10 + npcCount * MapNpc.SizeOnDisk + 3 * tileCount;
        ApiUtil.Assert(s.Offset - startOffset == expectedOffset, $"Map2D: Expected offset after layout to be {expectedOffset:x}, but it was {s.Offset - startOffset:x}");

        map.SerdesZones(s);
        map.SerdesEvents(mapping, map.MapType, s);
        map.SerdesNpcWaypoints(s);
        if (map.Events.Any())
            map.SerdesChains(s, 250);

        if (s.IsReading())
            map.Unswizzle();

        return map;
    }
}