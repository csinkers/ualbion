using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Assets
{
    public class MapData3D : BaseMapData
    {
        public override MapType MapType => MapType.ThreeD;
        public byte CeilingFlags { get; private set; }
        public LabyrinthDataId LabDataId { get; private set; }
        public byte Sound { get; private set; }
        public byte[] Contents { get; private set; }
        public byte[] Floors { get; private set; }
        public byte[] Ceilings { get; private set; }
        public IList<AutomapInfo> Automap { get; } = new List<AutomapInfo>();
        public byte[] AutomapGraphics { get; private set; }
        public IList<ushort> ActiveMapEvents { get; } = new List<ushort>();

        public static MapData3D Serdes(MapData3D existing, ISerializer s, string name, AssetInfo config)
        {
            var map = existing ?? new MapData3D();
            map.CeilingFlags = s.UInt8(nameof(CeilingFlags), map.CeilingFlags); // 0
            int npcCount = NpcCountTransform.Serdes("NpcCount", map.Npcs.Count, s.UInt8); // 1
            var _ = s.UInt8("MapType", (byte)map.MapType); // 2

            map.SongId = (SongId?)Tweak.Serdes(nameof(SongId), (byte?)map.SongId, s.UInt8); // 3
            map.Width = s.UInt8(nameof(Width), map.Width); // 4
            map.Height = s.UInt8(nameof(Height), map.Height); // 5
            map.LabDataId = s.EnumU8(nameof(LabDataId), map.LabDataId); // 6
            map.CombatBackgroundId = s.EnumU8(nameof(CombatBackgroundId), map.CombatBackgroundId); // 7 TODO: Verify this is combat background
            map.PaletteId = (PaletteId)StoreIncremented.Serdes(nameof(PaletteId), (byte)map.PaletteId, s.UInt8);
            map.Sound = StoreIncremented.Serdes(nameof(Sound), map.Sound, s.UInt8);

            for(int i = 0; i < npcCount; i++)
            {
                map.Npcs.TryGetValue(i, out var npc);
                npc = MapNpc.Serdes(i, npc, s);
                if (npc.Id != null)
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

            map.SerdesEvents(s);
            map.SerdesNpcWaypoints(s);
            map.SerdesAutomap(s);
            if(s.Mode != SerializerMode.Reading)
                map.Unswizzle();

            return map;
        }

        void SerdesAutomap(ISerializer s)
        {
            ushort automapInfoCount = s.UInt16("AutomapInfoCount", (ushort)Automap.Count);
            if (automapInfoCount != 0xffff)
            {
                s.List(Automap, automapInfoCount, AutomapInfo.Serdes);
                s.Check();
            }

            AutomapGraphics = s.ByteArray(nameof(AutomapGraphics), AutomapGraphics, 0x40);

            for(int i = 0; i < 64; i++)
            {
                if(s.Mode == SerializerMode.Reading)
                {
                    var eventId = s.UInt16(null, 0);
                    if (eventId != 0xffff)
                        ActiveMapEvents.Add(eventId);
                }
                else
                {
                    var eventId = ActiveMapEvents.Count <= i ? (ushort)0xffff : ActiveMapEvents[i];
                    s.UInt16(null, eventId);
                }
            }
            s.Check();
        }
    }
}
