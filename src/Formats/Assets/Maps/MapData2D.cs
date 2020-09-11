using System;
using System.Linq;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets.Maps
{
    public class MapData2D : BaseMapData
    {
        static readonly TilesetId[] OutdoorTilesets = { TilesetId.Outdoors, TilesetId.Outdoors2, TilesetId.Desert };

        public override MapType MapType => OutdoorTilesets.Contains(TilesetId) ? MapType.TwoDOutdoors : MapType.TwoD;
        public FlatMapFlags Flags { get; private set; } // Wait/Rest, Light-Environment, NPC converge range
        public byte Sound { get; private set; }
        public TilesetId TilesetId { get; private set; }
        public byte FrameRate { get; private set; }

        [JsonIgnore] public int[] Underlay { get; private set; }
        [JsonIgnore] public int[] Overlay { get; private set; }

        public byte[] RawLayout
        {
            get => FormatUtil.ToPacked(Width, Height, Underlay, Overlay);
            set => (Underlay, Overlay) = FormatUtil.FromPacked(Width, Height, value);
        }

        MapData2D(MapDataId id) : base(id) { }
        public static MapData2D Serdes(int id, MapData2D existing, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            var startOffset = s.Offset;
            var map = existing ?? new MapData2D((MapDataId)id);
            map.Flags = s.EnumU8(nameof(Flags), map.Flags); // 0
            int npcCount = s.Transform("NpcCount", map.Npcs.Count, S.UInt8, NpcCountTransform.Instance); // 1
            var _ = s.UInt8("MapType", (byte)map.MapType); // 2

            map.SongId = s.TransformEnumU8(nameof(SongId), map.SongId, TweakedConverter<SongId>.Instance); // 3
            map.Width = s.UInt8(nameof(Width), map.Width); // 4
            map.Height = s.UInt8(nameof(Height), map.Height); // 5

            map.TilesetId = s.TransformEnumU8(
                nameof(TilesetId),
                map.TilesetId,
                StoreIncrementedConverter<TilesetId>.Instance); //6

            map.CombatBackgroundId = s.EnumU8(nameof(CombatBackgroundId), map.CombatBackgroundId); // 7

            map.PaletteId = s.TransformEnumU8(
                nameof(PaletteId),
                map.PaletteId,
                StoreIncrementedConverter<PaletteId>.Instance);

            map.FrameRate = s.UInt8(nameof(FrameRate), map.FrameRate); //9

            for (int i = 0; i < npcCount; i++)
            {
                map.Npcs.TryGetValue(i, out var npc);
                npc = MapNpc.Serdes(i, npc, s);
                if (npc.ObjectNumber != 0 || npc.Id != null)
                    map.Npcs[i] = npc;
            }
            s.Check();

            if (s.Mode == SerializerMode.Reading)
                map.RawLayout = s.ByteArray("Layout", null, 3 * map.Width * map.Height);
            else
                s.ByteArray("Layout", map.RawLayout, 3 * map.Width * map.Height);

            s.Check();
            ApiUtil.Assert(s.Offset == startOffset + 10 + npcCount * MapNpc.SizeOnDisk + 3 * map.Width * map.Height);

            map.SerdesZones(s);
            map.SerdesEvents(s);
            map.SerdesNpcWaypoints(s);
            if (map.Events.Any())
                map.SerdesChains(s, 250);

            if (s.Mode == SerializerMode.Reading)
                map.Unswizzle();

            return map;
        }
    }
}
