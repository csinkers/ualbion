using System;
using System.Linq;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Maps
{
    public class MapData2D : BaseMapData
    {
        static readonly Base.Tileset[] OutdoorTilesets =
        { // TODO: Pull from config or infer from other data
            Base.Tileset.Outdoors,
            Base.Tileset.Outdoors2,
            Base.Tileset.Desert
        };
        public override MapType MapType => OutdoorTilesets.Any(x => x == TilesetId) ? MapType.TwoDOutdoors : MapType.TwoD;
        public FlatMapFlags Flags { get; private set; } // Wait/Rest, Light-Environment, NPC converge range
        public byte Sound { get; private set; }
        public TilesetId TilesetId { get; private set; }
        public byte FrameRate { get; private set; }

        [JsonIgnore] public int[] Underlay { get; private set; }
        [JsonIgnore] public int[] Overlay { get; private set; }

        public byte[] RawLayout
        {
            get => FormatUtil.ToPacked(Underlay, Overlay, 1);
            set => (Underlay, Overlay) = FormatUtil.FromPacked(value, -1);
        }

        public MapData2D() { } // For JSON
        public MapData2D(MapId id, byte width, byte height)
        {
            Id = id;
            Width = width;
            Height = height;
        }

        public static MapData2D Serdes(AssetInfo info, MapData2D existing, AssetMapping mapping, ISerializer s)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            if (s == null) throw new ArgumentNullException(nameof(s));

            var startOffset = s.Offset;
            var map = existing ?? new MapData2D { Id = info.AssetId };
            map.Flags = s.EnumU8(nameof(Flags), map.Flags); // 0
            map.OriginalNpcCount = s.UInt8(nameof(OriginalNpcCount), map.OriginalNpcCount); // 1
            int npcCount = NpcCountTransform.Instance.FromNumeric(map.OriginalNpcCount);
            var _ = s.UInt8("MapType", (byte)map.MapType); // 2 (always Map2D to start with, may shift to outdoors once we assign the tileset)

            map.SongId = SongId.SerdesU8(nameof(SongId), map.SongId, mapping, s); // 3
            map.Width = s.UInt8(nameof(Width), map.Width); // 4
            map.Height = s.UInt8(nameof(Height), map.Height); // 5
            map.TilesetId = TilesetId.SerdesU8(nameof(TilesetId), map.TilesetId, mapping, s); //6
            map.CombatBackgroundId = SpriteId.SerdesU8(nameof(CombatBackgroundId), map.CombatBackgroundId, AssetType.CombatBackground, mapping, s); // 7
            map.PaletteId = PaletteId.SerdesU8(nameof(PaletteId), map.PaletteId, mapping, s);
            map.FrameRate = s.UInt8(nameof(FrameRate), map.FrameRate); //9
            map.Npcs = s.List(
                nameof(Npcs),
                map.Npcs,
                npcCount,
                (n, x, s2) => MapNpc.Serdes(n, x, map.MapType, mapping, s2)).ToArray();

            if (s.IsReading())
                map.RawLayout = s.Bytes("Layout", null, 3 * map.Width * map.Height);
            else
                s.Bytes("Layout", map.RawLayout, 3 * map.Width * map.Height);

            s.Check();
            ApiUtil.Assert(s.Offset == startOffset + 10 + npcCount * MapNpc.SizeOnDisk + 3 * map.Width * map.Height);

            map.SerdesZones(s);
            map.SerdesEvents(mapping, s);
            map.SerdesNpcWaypoints(s);
            if (map.Events.Any())
                map.SerdesChains(s, 250);

            if (s.IsReading())
                map.Unswizzle();

            return map;
        }
    }
}
