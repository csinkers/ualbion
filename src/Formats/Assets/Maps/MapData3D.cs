using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Maps
{
    public class MapData3D : BaseMapData
    {
        public override MapType MapType => MapType.ThreeD;
        public Map3DFlags Flags { get; private set; }
        public LabyrinthId LabDataId { get; private set; }
        public SongId AmbientSongId { get; private set; }
        public byte[] Contents { get; private set; }
        public byte[] Floors { get; private set; }
        public byte[] Ceilings { get; private set; }
        public IList<AutomapInfo> Automap { get; } = new List<AutomapInfo>();
        public byte[] AutomapGraphics { get; private set; }

        const int WallOffset = 100;

        public byte[] BuildWallArray() => Contents.Select(x => (byte)(x >= WallOffset ? x - WallOffset : 0)).ToArray();
        public byte[] BuildObjectArray() => Contents.Select(x => x < WallOffset ? x : (byte)0).ToArray();
        public byte GetWall(int index)
        {
            if (index < 0 || index > Contents.Length) return 0;
            var contents = Contents[index];
            return (byte)(contents >= WallOffset ? contents - WallOffset : 0);
        }

        public byte GetObject(int index)
        {
            if (index < 0 || index > Contents.Length) return 0;
            var contents = Contents[index];
            return contents < WallOffset ? contents : (byte)0;
        }

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
}
