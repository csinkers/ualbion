using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace UAlbion.Formats.Parsers
{
    public class MapData3D
    {
        public byte CeilingFlags { get; set; }
        public byte Width { get; set; }
        public byte Height { get; set; }
        public int SongId { get; set; }
        public int LabDataId { get; set; }
        public byte Unk7 { get; set; }
        public int PaletteId { get; set; }
        public int Sound { get; set; }
        public int[] Contents { get; set; }
        public int[] Floors { get; set; }
        public int[] Ceilings { get; set; }
        public IList<MapNpc> Npcs { get; } = new List<MapNpc>();
        public IList<MapEventZone> Zones { get; } = new List<MapEventZone>();
        public IList<MapEvent> Events { get; } = new List<MapEvent>();
        public IList<AutomapInfo> Automap { get; } = new List<AutomapInfo>();
        public byte[] AutomapGraphics { get; set; }
        public IList<ushort> ActiveMapEvents { get; } = new List<ushort>();

        public static MapData3D Load(BinaryReader br, string name)
        {
            var startPosition = br.BaseStream.Position;

            var map = new MapData3D();
            map.CeilingFlags = br.ReadByte(); // 0
            int npcCount = br.ReadByte(); // 1
            if (npcCount == 0) npcCount = 0x20;
            else if (npcCount == 0x40) npcCount = 0x60;

            var mapType = br.ReadByte(); // 2
            Debug.Assert(1 == mapType); // 1 = 3D, 2 = 2D

            map.SongId    = br.ReadByte() - 1; //3
            map.Width     = br.ReadByte(); //4
            map.Height    = br.ReadByte(); //5
            map.LabDataId = br.ReadByte() - 1; //6
            map.Unk7 = br.ReadByte(); //7 Possibly combat background??
            map.PaletteId = br.ReadByte() - 1; //8
            map.Sound = br.ReadByte() - 1; //9

            for (int i = 0; i < npcCount; i++)
                map.Npcs.Add(MapNpc.Load(br));

            map.Contents = new int[map.Width * map.Height];
            map.Floors = new int[map.Width * map.Height];
            map.Ceilings = new int[map.Width * map.Height];
            for (int i = 0; i < map.Width * map.Height; i++)
            {
                map.Contents[i] = br.ReadByte();
                map.Floors[i]   = br.ReadByte();
                map.Ceilings[i] = br.ReadByte();
            }

            int zoneCount = br.ReadUInt16();
            for (int i = 0; i < zoneCount; i++)
                map.Zones.Add(MapEventZone.LoadGlobalZone(br));

            for (int j = 0; j < map.Height; j++)
            {
                zoneCount = br.ReadUInt16();
                for (int i = 0; i < zoneCount; i++)
                    map.Zones.Add(MapEventZone.LoadZone(br, (ushort)j));
            }

            int eventCount = br.ReadUInt16();
            for (int i = 0; i < eventCount; i++)
                map.Events.Add(MapEvent.Load(br, i));

            foreach(var npc in map.Npcs)
                npc.LoadWaypoints(br);

            int automapInfoCount = br.ReadUInt16();
            for (int i = 0; i < automapInfoCount; i++)
                map.Automap.Add(AutomapInfo.Load(br));

            map.AutomapGraphics = br.ReadBytes(0x40);

            for(int i = 0; i < 64; i++)
            {
                var eventId = br.ReadUInt16();
                if (eventId == 0xffff)
                    continue;

                map.ActiveMapEvents.Add(eventId);
            }

            return map;
        }

        public class AutomapInfo
        {
            public byte X;
            public byte Y;
            public byte Unk2;
            public byte Unk3;
            public string Name; // Map length = 15

            public static AutomapInfo Load(BinaryReader br)
            {
                var i = new AutomapInfo();
                i.X = br.ReadByte();
                i.Y = br.ReadByte();
                i.Unk2 = br.ReadByte();
                i.Unk3 = br.ReadByte();
                var nameBytes = br.ReadBytes(15);
                i.Name = Encoding.ASCII.GetString(nameBytes);
                return i;
            }
        }
    }
}
