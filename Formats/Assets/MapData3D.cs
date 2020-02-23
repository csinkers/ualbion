using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets
{
    public class MapData3D : IMapData
    {
        public MapType MapType => MapType.ThreeD;
        public byte CeilingFlags { get; private set; }
        public byte Width { get; private set; }
        public byte Height { get; private set; }
        public SongId? SongId { get; private set; }
        public LabyrinthDataId LabDataId { get; private set; }
        public CombatBackgroundId CombatBackgroundId { get; private set; }
        public PaletteId PaletteId { get; private set; }
        public byte Sound { get; private set; }
        public byte[] Contents { get; private set; }
        public byte[] Floors { get; private set; }
        public byte[] Ceilings { get; private set; }
        public IList<MapNpc> Npcs { get; } = new List<MapNpc>();
        public IList<MapEventZone> Zones { get; } = new List<MapEventZone>();
        public IDictionary<int, MapEventZone[]> ZoneLookup { get; } = new Dictionary<int, MapEventZone[]>(); 
        public IDictionary<TriggerType, MapEventZone[]> ZoneTypeLookup { get; } = new Dictionary<TriggerType, MapEventZone[]>();
        public IList<EventNode> Events { get; } = new List<EventNode>();
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

            s.List(map.Npcs, npcCount, MapNpc.Serdes);
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

            int zoneCount = s.UInt16("ZoneCount", (ushort)map.Zones.Count(x => x.Global));
            s.List(map.Zones, zoneCount, (i, x,serializer) => MapEventZone.Serdes(x, serializer, 0xff));
            s.Check();

            int zoneOffset = zoneCount;
            for (byte y = 0; y < map.Height; y++)
            {
                zoneCount = s.UInt16("RowZones", (ushort)map.Zones.Count(x => x.Y == y && !x.Global));
                s.List(map.Zones, zoneCount, zoneOffset, (i, x, s) => MapEventZone.Serdes(x, s, y));
                zoneOffset += zoneCount;
            }

            if (s.Mode == SerializerMode.Reading && s.IsComplete() || s.Mode != SerializerMode.Reading && map.AutomapGraphics == null)
            {
                Debug.Assert(map.Zones.Count == 0);
                return map;
            }

            ushort eventCount = s.UInt16("EventCount", (ushort)map.Events.Count);
            s.List(map.Events, eventCount, EventNode.Serdes);
            s.Check();

            foreach(var npc in map.Npcs)
                if (npc.Id != 0)
                    npc.LoadWaypoints(s);
            s.Check();

            ushort automapInfoCount = s.UInt16("AutomapInfoCount", (ushort)map.Automap.Count);
            if (automapInfoCount != 0xffff)
            {
                s.List(map.Automap, automapInfoCount, AutomapInfo.Serdes);
                s.Check();
            }

            map.AutomapGraphics = s.ByteArray(nameof(AutomapGraphics), map.AutomapGraphics, 0x40);

            for(int i = 0; i < 64; i++)
            {
                if(s.Mode == SerializerMode.Reading)
                {
                    var eventId = s.UInt16(null, 0);
                    if (eventId != 0xffff)
                        map.ActiveMapEvents.Add(eventId);
                }
                else
                {
                    var eventId = map.ActiveMapEvents.Count <= i ? (ushort)0xffff : map.ActiveMapEvents[i];
                    s.UInt16(null, eventId);
                }
            }
            s.Check();

            // Resolve event indices to pointers
            foreach (var mapEvent in map.Events)
            {
                if (mapEvent.NextEventId.HasValue && mapEvent.NextEventId < map.Events.Count)
                    mapEvent.NextEvent = map.Events[mapEvent.NextEventId.Value];

                if (mapEvent is BranchNode q && q.NextEventWhenFalseId.HasValue && q.NextEventWhenFalseId < map.Events.Count)
                    q.NextEventWhenFalse = map.Events[q.NextEventWhenFalseId.Value];
            }

            foreach(var npc in map.Npcs)
                if (npc.Id != 0 && npc.EventNumber.HasValue)
                    npc.EventChain = map.Events[npc.EventNumber.Value];

            foreach(var zone in map.Zones)
                if (zone.EventNumber != 65535)
                    zone.EventNode = map.Events[zone.EventNumber];

            foreach (var position in map.Zones.GroupBy(x => x.Y * map.Width + x.X))
                map.ZoneLookup[position.Key] = position.ToArray();

            foreach (var triggerType in map.Zones.Where(x => x.Global || x.Y == 0).GroupBy(x => x.Trigger))
                map.ZoneTypeLookup[triggerType.Key] = triggerType.ToArray();

            return map;
        }

        public static MapData3D Load(BinaryReader br, long streamLength, string name)
        {
            var startPosition = br.BaseStream.Position;

            var map = new MapData3D();
            map.CeilingFlags = br.ReadByte(); // 0
            int npcCount = br.ReadByte();     // 1
            if (npcCount == 0) npcCount = 0x20;
            else if (npcCount == 0x40) npcCount = 0x60;

            var mapType = br.ReadByte(); // 2
            Debug.Assert(1 == mapType);  // 1 = 3D, 2 = 2D

            map.SongId    = (SongId?)FormatUtil.Tweak(br.ReadByte()); // 3
            map.Width     = br.ReadByte(); // 4
            map.Height    = br.ReadByte(); // 5
            byte labData  = br.ReadByte(); // 6
            map.LabDataId = (LabyrinthDataId)labData;

            map.CombatBackgroundId = (CombatBackgroundId)br.ReadByte(); // 7 TODO: Verify this is combat background
            map.PaletteId = (PaletteId)br.ReadByte() - 1; // 8
            map.Sound = (byte)(br.ReadByte() - 1); // 9 // TODO: Check 0 handling etc

            for (int i = 0; i < npcCount; i++)
                map.Npcs.Add(MapNpc.Load(br));
            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            map.Contents = new byte[map.Width * map.Height];
            map.Floors   = new byte[map.Width * map.Height];
            map.Ceilings = new byte[map.Width * map.Height];
            for (int i = 0; i < map.Width * map.Height; i++)
            {
                map.Contents[i] = br.ReadByte();
                map.Floors[i]   = br.ReadByte();
                map.Ceilings[i] = br.ReadByte();
            }
            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            int zoneCount = br.ReadUInt16();
            for (int i = 0; i < zoneCount; i++)
                map.Zones.Add(MapEventZone.LoadZone(br, 0xff));
            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            for (byte j = 0; j < map.Height; j++)
            {
                zoneCount = br.ReadUInt16();
                for (int i = 0; i < zoneCount; i++)
                    map.Zones.Add(MapEventZone.LoadZone(br, j));
            }

            if (br.BaseStream.Position == startPosition + streamLength)
            {
                Debug.Assert(map.Zones.Count == 0);
                return map;
            }

            int eventCount = br.ReadUInt16();
            for (int i = 0; i < eventCount; i++)
                map.Events.Add(EventNode.Serdes(i, null, new GenericBinaryReader(br, EventNode.SizeInBytes)));
            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            foreach(var npc in map.Npcs)
                if (npc.Id != 0)
                    npc.LoadWaypoints(br);
            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            int automapInfoCount = br.ReadUInt16();
            if (automapInfoCount != 0xffff)
            {
                for (int i = 0; i < automapInfoCount; i++)
                    map.Automap.Add(AutomapInfo.Load(br));
                Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);
            }

            map.AutomapGraphics = br.ReadBytes(0x40);

            for(int i = 0; i < 64; i++)
            {
                var eventId = br.ReadUInt16();
                if (eventId == 0xffff)
                    continue;

                map.ActiveMapEvents.Add(eventId);
            }
            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            // Resolve event indices to pointers
            foreach (var mapEvent in map.Events)
            {
                if (mapEvent.NextEventId.HasValue && mapEvent.NextEventId < map.Events.Count)
                    mapEvent.NextEvent = map.Events[mapEvent.NextEventId.Value];

                if (mapEvent is BranchNode q && q.NextEventWhenFalseId.HasValue && q.NextEventWhenFalseId < map.Events.Count)
                    q.NextEventWhenFalse = map.Events[q.NextEventWhenFalseId.Value];
            }

            foreach(var npc in map.Npcs)
                if (npc.Id != 0 && npc.EventNumber.HasValue)
                    npc.EventChain = map.Events[npc.EventNumber.Value];

            foreach(var zone in map.Zones)
                if (zone.EventNumber != 65535)
                    zone.EventNode = map.Events[zone.EventNumber];

            foreach (var position in map.Zones.GroupBy(x => x.Y * map.Width + x.X))
                map.ZoneLookup[position.Key] = position.ToArray();

            foreach (var triggerType in map.Zones.Where(x => x.Global || x.Y == 0).GroupBy(x => x.Trigger))
                map.ZoneTypeLookup[triggerType.Key] = triggerType.ToArray();

            return map;
        }
    }
}
