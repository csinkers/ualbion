using System;
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
        public SongId SongId { get; private set; }
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

        class NpcCountTransform : IConverter<byte, int>
        {
            public static readonly NpcCountTransform Instance = new NpcCountTransform();
            private NpcCountTransform() { }
            public byte ToPersistent(int memory) => memory switch
            {
                0x20 => (byte)0,
                0x60 => (byte)0x40,
                _ when memory > 0xff => throw new InvalidOperationException("Too many NPCs in map"),
                _ => (byte)memory
            };

            public int ToMemory(byte persistent) => persistent switch
            {
                0 => 0x20,
                0x40 => 0x60,
                _ => persistent
            };
        }

        public static MapData3D Serdes(MapData3D existing, ISerializer s, string name, AssetInfo config)
        {
            var startPosition = s.Offset;

            var map = existing ?? new MapData3D();
            s.Dynamic(map, nameof(CeilingFlags)); // 0
            int npcCount = s.Transform("NpcCount", map.Npcs.Count, s.UInt8, NpcCountTransform.Instance); // 1
            var _ = s.UInt8("MapType", (byte)map.MapType); // 2

            map.SongId = (SongId)(s.Transform<byte, byte?>(nameof(SongId), (byte)map.SongId, s.UInt8, Tweak.Instance) ?? 0); // 3
            s.Dynamic(map, nameof(Width)); // 4
            s.Dynamic(map, nameof(Height)); // 5
            s.Dynamic(map, nameof(LabDataId)); // 6
            s.Dynamic(map, nameof(CombatBackgroundId)); // 7 TODO: Verify this is combat background
            map.PaletteId = (PaletteId)s.Transform<byte, byte>(nameof(PaletteId), (byte)map.PaletteId, s.UInt8, StoreIncremented.Instance);
            map.Sound = s.Transform<byte, byte>(nameof(Sound), map.Sound, s.UInt8, StoreIncremented.Instance);

            s.List(map.Npcs, npcCount, (i, x, s) => MapNpc.Serdes(x, s));
            for (int i = map.Npcs.Count - 1; i >= 0; i--)
                if (map.Npcs[i].Id == 0)
                    map.Npcs.RemoveAt(i);

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

            int zoneCount = map.Zones.Count - map.Height;
            s.List(map.Zones, zoneCount, (i, x,s) => MapEventZone.Serdes(x, s, 0xffff));
            s.Check();

            int zoneOffset = zoneCount;
            for (int j = 0; j < map.Height; j++)
            {
                zoneCount = map.Zones.Count(x => x.Y == j);
                s.List(map.Zones, zoneCount, zoneOffset, (i, x, s) => MapEventZone.Serdes(x, s,  (ushort)(i - zoneOffset)));
                zoneOffset += zoneCount;
            }

            if (!map.Events.Any()) // if (s.Offset == startPosition + streamLength)
            {
                Debug.Assert(map.Zones.Count == 0);
                return map;
            }

            int eventCount = map.Events.Count;
            s.List(map.Events, eventCount, EventNode.Serdes);
            s.Check();

            foreach(var npc in map.Npcs)
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
                var eventId = s.UInt16(null, i >= map.ActiveMapEvents.Count ? (ushort)0xffff : map.ActiveMapEvents[i]);
                if (eventId != 0xffff && i < map.ActiveMapEvents.Count)
                    map.ActiveMapEvents.Add(eventId);
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
                if (npc.EventNumber.HasValue)
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

            map.SongId    = (SongId)br.ReadByte() - 1; // 3
            map.Width     = br.ReadByte(); // 4
            map.Height    = br.ReadByte(); // 5
            byte labData  = br.ReadByte(); // 6
            map.LabDataId = (LabyrinthDataId)labData;

            map.CombatBackgroundId = (CombatBackgroundId)br.ReadByte(); // 7 TODO: Verify this is combat background
            map.PaletteId = (PaletteId)br.ReadByte() - 1; // 8
            map.Sound = (byte)(br.ReadByte() - 1); // 9 // TODO: Check 0 handling etc

            for (int i = 0; i < npcCount; i++)
            {
                var npc = MapNpc.Load(br);
                if (npc.Id == 0) continue;
                map.Npcs.Add(npc);
            }
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
                map.Zones.Add(MapEventZone.LoadZone(br, 0xffff));
            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            for (int j = 0; j < map.Height; j++)
            {
                zoneCount = br.ReadUInt16();
                for (int i = 0; i < zoneCount; i++)
                    map.Zones.Add(MapEventZone.LoadZone(br, (ushort)j));
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
            foreach (var mapEvent in map.Events.OfType<EventNode>())
            {
                if (mapEvent.NextEventId.HasValue && mapEvent.NextEventId < map.Events.Count)
                    mapEvent.NextEvent = map.Events[mapEvent.NextEventId.Value];

                if (mapEvent is BranchNode q && q.NextEventWhenFalseId.HasValue && q.NextEventWhenFalseId < map.Events.Count)
                    q.NextEventWhenFalse = map.Events[q.NextEventWhenFalseId.Value];
            }

            foreach(var npc in map.Npcs)
                if (npc.EventNumber.HasValue)
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
