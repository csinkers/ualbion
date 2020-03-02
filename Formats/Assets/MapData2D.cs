using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets
{
    public class MapData2D : BaseMapData
    {
        public override MapType MapType => MapType.TwoD;
        public byte Unk0 { get; private set; } // Wait/Rest, Light-Environment, NPC converge range
        public byte Sound { get; private set; }
        public TilesetId TilesetId { get; private set; }
        public byte FrameRate { get; private set; }

        public int[] Underlay { get; private set; }
        public int[] Overlay { get; private set; }

        public static MapData2D Serdes(MapData2D existing, ISerializer s)
        {
            var startOffset = s.Offset;
            var map = existing ?? new MapData2D();
            map.Unk0 = s.UInt8(nameof(Unk0), map.Unk0); // 0
            int npcCount = s.Transform("NpcCount", map.Npcs.Count, s.UInt8, NpcCountTransform.Instance); // 1
            var _ = s.UInt8("MapType", (byte)map.MapType); // 2

            map.SongId = (SongId?)Tweak.Serdes(nameof(SongId), (byte?)map.SongId, s.UInt8); // 3
            map.Width = s.UInt8(nameof(Width), map.Width); // 4
            map.Height = s.UInt8(nameof(Height), map.Height); // 5
            map.TilesetId = (TilesetId)StoreIncremented.Serdes(nameof(TilesetId), (byte)map.TilesetId, s.UInt8);  //6
            map.CombatBackgroundId = s.EnumU8(nameof(CombatBackgroundId), map.CombatBackgroundId); // 7
            map.PaletteId = (PaletteId)StoreIncremented.Serdes(nameof(PaletteId), (byte)map.PaletteId, s.UInt8);
            map.FrameRate = s.UInt8(nameof(FrameRate), map.FrameRate); //9

            s.List(map.Npcs, npcCount, MapNpc.Serdes);
            s.Check();

            map.Underlay ??= new int[map.Width * map.Height];
            map.Overlay ??= new int[map.Width * map.Height];
            for (int i = 0; i < map.Width * map.Height; i++)
            {
                ushort underlay = (ushort)(map.Underlay[i] + 2);
                ushort overlay = (ushort)(map.Overlay[i] + 2);

                byte b1 = (byte)(overlay >> 4);
                byte b2 = (byte)(((overlay & 0xf) << 4) | ((underlay & 0xf00) >> 8));
                byte b3 = (byte)(underlay & 0xff);

                b1 = s.UInt8(null, b1);
                b2 = s.UInt8(null, b2);
                b3 = s.UInt8(null, b3);

                map.Overlay[i] = (b1 << 4) + (b2 >> 4) - 2;
                map.Underlay[i] = ((b2 & 0x0F) << 8) + b3 - 2;
            }
            s.Check();
            ApiUtil.Assert(s.Offset == startOffset + 10 + npcCount * MapNpc.SizeOnDisk + 3 * map.Width * map.Height);

            map.SerdesZones(s);
            map.SerdesEvents(s);
            map.SerdesNpcWaypoints(s);
            if (s.Mode == SerializerMode.Reading)
                map.Unswizzle();
            return map;
        }
    }
}
