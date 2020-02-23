using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets
{
    public class MapData2D : IMapData
    {
        public MapType MapType => MapType.TwoD;
        public byte Unk0 { get; private set; } // Wait/Rest, Light-Environment, NPC converge range
        public SongId? SongId { get; private set; }
        public byte Sound { get; private set; }
        public byte Width { get; private set; }
        public byte Height { get; private set; }
        public TilesetId TilesetId { get; private set; }
        public CombatBackgroundId CombatBackgroundId { get; private set; }
        public PaletteId PaletteId { get; private set; }
        public byte FrameRate { get; private set; }

        public int[] Underlay { get; private set; }
        public int[] Overlay { get; private set; }
        public IList<MapNpc> Npcs { get; } = new List<MapNpc>();
        public IList<EventNode> Events { get; } = new List<EventNode>();
        public IList<MapEventZone> Zones { get; } = new List<MapEventZone>();
        public IDictionary<int, MapEventZone[]> ZoneLookup { get; } = new Dictionary<int, MapEventZone[]>(); 
        public IDictionary<TriggerType, MapEventZone[]> ZoneTypeLookup { get; } = new Dictionary<TriggerType, MapEventZone[]>();

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

                map.Overlay[i]  = (b1 << 4) + (b2 >> 4) - 2;
                map.Underlay[i] = ((b2 & 0x0F) << 8) + b3 - 2;
            }
            s.Check();
            Debug.Assert(s.Offset == startOffset + 10 + npcCount * MapNpc.SizeOnDisk + 3 * map.Width * map.Height);

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

            ushort eventCount = s.UInt16("EventCount", (ushort)map.Events.Count);
            s.List(map.Events, eventCount, EventNode.Serdes);
            s.Check();

            foreach (var npc in map.Npcs)
                if (npc.Id != 0)
                    npc.LoadWaypoints(s);

            // Resolve event indices to pointers
            foreach (var mapEvent in map.Events)
            {
                if (mapEvent.NextEventId.HasValue)
                    mapEvent.NextEvent = map.Events[mapEvent.NextEventId.Value];

                if (mapEvent is BranchNode q && q.NextEventWhenFalseId.HasValue)
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

        public static MapData2D Load(BinaryReader br, long streamLength, string name)
        {
            var startPosition = br.BaseStream.Position;

            var map = new MapData2D();
            map.Unk0 = br.ReadByte(); // 0
            int npcCount = br.ReadByte(); // 1
            if (npcCount == 0) npcCount = 0x20;
            else if (npcCount == 0x40) npcCount = 0x60;

            var mapType = br.ReadByte(); // 2
            Debug.Assert(2 == mapType); // 1 = 3D, 2 = 2D

            map.SongId = (SongId?)FormatUtil.Tweak(br.ReadByte()); //3
            map.Width = br.ReadByte(); //4
            map.Height = br.ReadByte(); //5
            map.TilesetId = (TilesetId?)FormatUtil.Tweak(br.ReadByte()) ?? throw new FormatException("Invalid tile-set id encountered"); //6
            map.CombatBackgroundId = (CombatBackgroundId)br.ReadByte(); //7
            map.PaletteId = (PaletteId)(FormatUtil.Tweak(br.ReadByte()) ?? throw new FormatException("Invalid palette id encountered")); //8
            map.FrameRate = br.ReadByte(); //9

            for (int i = 0; i < npcCount; i++)
            {
                var npc = MapNpc.Load(br);
                map.Npcs.Add(npc);
            }
            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            map.Underlay = new int[map.Width * map.Height];
            map.Overlay = new int[map.Width * map.Height];
            for (int i = 0; i < map.Width * map.Height; i++)
            {
                byte b1 = br.ReadByte();
                byte b2 = br.ReadByte();
                byte b3 = br.ReadByte();

                int overlay = (b1 << 4) + (b2 >> 4) - 2;
                int underlay = ((b2 & 0x0F) << 8) + b3 - 2;
                map.Overlay[i] = overlay;
                map.Underlay[i] = underlay; 
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
            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            int eventCount = br.ReadUInt16();
            for (int i = 0; i < eventCount; i++)
                map.Events.Add(EventNode.Serdes(i, null, new GenericBinaryReader(br, EventNode.SizeInBytes)));

            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            foreach (var npc in map.Npcs)
                if (npc.Id != 0)
                    npc.LoadWaypoints(br);

            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            // Resolve event indices to pointers
            foreach (var mapEvent in map.Events)
            {
                if (mapEvent.NextEventId.HasValue)
                    mapEvent.NextEvent = map.Events[mapEvent.NextEventId.Value];

                if (mapEvent is BranchNode q && q.NextEventWhenFalseId.HasValue)
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
