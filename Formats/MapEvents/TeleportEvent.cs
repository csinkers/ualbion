using System.Diagnostics;
using System.IO;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class TeleportEvent : IEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type)
        {
            var e = new TeleportEvent
            {
                X = br.ReadByte(), // +1
                Y = br.ReadByte(), // +2
                Direction = br.ReadByte(), // +3
                Unk4 = br.ReadByte(), // +4
                Unk5 = br.ReadByte(), // +5
                MapId = (MapDataId) br.ReadUInt16(), // +6
                Unk8 = br.ReadUInt16(), // +8
            };
            Debug.Assert(e.Unk4 == 0
                         || e.Unk4 == 1
                         || e.Unk4 == 2
                         || e.Unk4 == 3
                         || e.Unk4 == 6
                         || e.Unk4 == 106
                         || e.Unk4 == 255); // Always 255 in maps
            Debug.Assert(e.Unk8 == 0);
            return new EventNode(id, e);
        }

        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Direction { get; set; } // 0,1,2,3,255
        public MapDataId MapId { get; set; } // 0 = stay on current map

        public byte Unk4 { get; set; } // 255 on 2D maps, (1,6) on 3D maps
        public byte Unk5 { get; set; } // 2,3,4,5,6,8,9
        ushort Unk8 { get; set; } 
        public override string ToString() => $"teleport {MapId} <{X}, {Y}> Dir:{Direction} ({Unk4} {Unk5})";
    }
}
