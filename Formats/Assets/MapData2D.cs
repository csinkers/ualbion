using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets
{
    public class MapData2D
    {
        public byte Unk0 { get; private set; } // Wait/Rest, Light-Environment, NPC converge range
        public byte Sound { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TilesetId { get; private set; }
        public int CombatBackgroundId { get; private set; }
        public int PaletteId { get; private set; }
        public byte FrameRate { get; private set; }

        public int[] Underlay { get; private set; }
        public int[] Overlay { get; private set; }
        public IList<MapNpc> Npcs { get; } = new List<MapNpc>();
        public IList<IEventNode> Events { get; } = new List<IEventNode>();
        public IList<MapEventZone> Zones { get; } = new List<MapEventZone>();
        public IDictionary<int, MapEventZone[]> ZoneLookup { get; } = new Dictionary<int, MapEventZone[]>(); 
        public IDictionary<TriggerType, MapEventZone[]> ZoneTypeLookup { get; } = new Dictionary<TriggerType, MapEventZone[]>();

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

            map.Sound = br.ReadByte(); //3
            map.Width = br.ReadByte(); //4
            map.Height = br.ReadByte(); //5
            map.TilesetId = FormatUtil.Tweak(br.ReadByte()) ?? throw new FormatException("Invalid tile-set id encountered"); //6
            map.CombatBackgroundId = br.ReadByte(); //7
            map.PaletteId = FormatUtil.Tweak(br.ReadByte()) ?? throw new FormatException("Invalid palette id encountered"); //8
            map.FrameRate = br.ReadByte(); //9

            for (int i = 0; i < npcCount; i++)
            {
                var npc = MapNpc.Load(br);
                if (npc.Id == 0) continue;
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
                map.Zones.Add(MapEventZone.LoadGlobalZone(br));
            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            for (int j = 0; j < map.Height; j++)
            {
                zoneCount = br.ReadUInt16();
                for (int i = 0; i < zoneCount; i++)
                    map.Zones.Add(MapEventZone.LoadZone(br, (ushort)j));
            }
            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            int eventCount = br.ReadUInt16();
            for (int i = 0; i < eventCount; i++)
                map.Events.Add(EventNode.Translate(null, new GenericBinaryReader(br, EventNode.SizeInBytes), i));

            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            foreach (var npc in map.Npcs)
                npc.LoadWaypoints(br);

            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            // Resolve event indices to pointers
            foreach (var mapEvent in map.Events.OfType<EventNode>())
            {
                if (mapEvent.NextEventId.HasValue)
                    mapEvent.NextEvent = map.Events[mapEvent.NextEventId.Value];

                if (mapEvent is BranchNode q && q.NextEventWhenFalseId.HasValue)
                    q.NextEventWhenFalse = map.Events[q.NextEventWhenFalseId.Value];
            }

            foreach(var npc in map.Npcs)
                if (npc.EventNumber.HasValue)
                    npc.EventChain = map.Events[npc.EventNumber.Value];

            foreach(var zone in map.Zones)
                if (zone.EventNumber != 65535)
                    zone.Event = map.Events[zone.EventNumber];

            foreach (var position in map.Zones.GroupBy(x => x.Y * map.Width + x.X))
                map.ZoneLookup[position.Key] = position.ToArray();

            foreach (var triggerType in map.Zones.Where(x => x.Global || x.Y == 0).GroupBy(x => x.Trigger))
                map.ZoneTypeLookup[triggerType.Key] = triggerType.ToArray();

            return map;
        }
    }
}
