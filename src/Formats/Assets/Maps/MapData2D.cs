using System;
using System.Linq;
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

        public int[] Underlay { get; private set; }
        public int[] Overlay { get; private set; }

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

            map.Underlay ??= new int[map.Width * map.Height];
            map.Overlay ??= new int[map.Width * map.Height];
            for (int i = 0; i < map.Width * map.Height; i++)
            {
                ushort underlay = (ushort)(map.Underlay[i] + 1);
                ushort overlay = (ushort)(map.Overlay[i] + 1);

                byte b1 = (byte)(overlay >> 4);
                byte b2 = (byte)(((overlay & 0xf) << 4) | ((underlay & 0xf00) >> 8));
                byte b3 = (byte)(underlay & 0xff);

                b1 = s.UInt8(null, b1);
                b2 = s.UInt8(null, b2);
                b3 = s.UInt8(null, b3);

                map.Overlay[i] = (b1 << 4) + (b2 >> 4);
                map.Underlay[i] = ((b2 & 0x0F) << 8) + b3;
            }
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
