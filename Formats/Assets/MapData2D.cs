using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets
{
    public class MapData2D
    {
        public int[] Underlay { get; set; }
        public int[] Overlay { get; set; }
        public IList<MapNpc> Npcs { get; } = new List<MapNpc>();
        public IList<MapEvent> Events { get; } = new List<MapEvent>();
        public IList<MapEventZone> Zones { get; } = new List<MapEventZone>();
        public int[] ZoneLookup { get; private set; }

        public byte Unk0 { get; set; } // Wait/Rest, Light-Environment, NPC converge range
        public byte Sound { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int TilesetId { get; set; }
        public int CombatBackgroundId { get; set; }
        public int PaletteId { get; set; }
        public byte FrameRate { get; set; }

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
                map.Events.Add(MapEvent.Load(br, i));

            foreach (var mapEvent in map.Events)
            {
                if (mapEvent.NextEventId.HasValue)
                    mapEvent.NextEvent = map.Events[mapEvent.NextEventId.Value];

                if (mapEvent is QueryEvent q && q.FalseEventId.HasValue)
                    q.FalseEvent = map.Events[q.FalseEventId.Value];
            }

            foreach(var zone in map.Zones)
                zone.Event = map.Events[zone.EventNumber];

            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            foreach (var npc in map.Npcs)
                npc.LoadWaypoints(br);

            Debug.Assert(br.BaseStream.Position <= startPosition + streamLength);

            map.ZoneLookup = Enumerable.Repeat(-1, map.Width * map.Height).ToArray();
            for (int i = 0; i < map.Zones.Count; i++)
            {
                var z = map.Zones[i];
                map.ZoneLookup[z.Y * map.Width + z.X] = i;
            }
            return map;
        }
    }
}
