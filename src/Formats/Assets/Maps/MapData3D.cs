using System;
using System.Collections.Generic;
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

        MapData3D(MapId id) : base(id) { }
        public static MapData3D Serdes(int id, MapData3D existing, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            var map = existing ?? new MapData3D((MapId)id);
            map.Flags = s.EnumU8(nameof(Flags), map.Flags); // 0
            int npcCount = s.Transform("NpcCount", map.Npcs.Count, S.UInt8, NpcCountTransform.Instance); // 1
            var _ = s.UInt8("MapType", (byte)map.MapType); // 2

            map.SongId = SongId.SerdesU8(nameof(SongId), map.SongId, mapping, s); // 3
            map.Width = s.UInt8(nameof(Width), map.Width); // 4
            map.Height = s.UInt8(nameof(Height), map.Height); // 5
            map.LabDataId = LabyrinthId.SerdesU8(nameof(LabDataId), map.LabDataId, mapping, s); // 6
            map.CombatBackgroundId = SpriteId.SerdesU8(nameof(CombatBackgroundId), map.CombatBackgroundId, AssetType.CombatBackground, mapping, s); // 7 TODO: Verify this is combat background
            map.PaletteId = PaletteId.SerdesU8(nameof(PaletteId), map.PaletteId, mapping, s);
            map.AmbientSongId = SongId.SerdesU8(nameof(AmbientSongId), map.AmbientSongId, mapping, s);

            for (int i = 0; i < npcCount; i++)
            {
                map.Npcs.TryGetValue(i, out var npc);
                npc = MapNpc.Serdes(i, npc, MapType.ThreeD, mapping, s);
                if (!npc.Id.IsNone)
                    map.Npcs[i] = npc;
            }
            s.Check();

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

            if (s.Mode == SerializerMode.Reading && s.IsComplete() || s.Mode != SerializerMode.Reading && map.AutomapGraphics == null)
            {
                ApiUtil.Assert(map.Zones.Count == 0);
                return map;
            }

            map.SerdesEvents(mapping, s);
            map.SerdesNpcWaypoints(s);
            map.SerdesAutomap(s);
            map.SerdesChains(s, 64);

            if (s.Mode == SerializerMode.Reading)
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

            AutomapGraphics = s.ByteArray(nameof(AutomapGraphics), AutomapGraphics, 0x40);
            s.Check();
        }
    }
}
