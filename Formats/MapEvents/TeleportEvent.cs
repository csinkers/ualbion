using System.Diagnostics;
using System.IO;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class TeleportEvent : MapEvent
    {
        public TeleportEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            X = br.ReadByte(); // +1
            Y = br.ReadByte(); // +2
            Direction = br.ReadByte(); // +3
            Unk4 = br.ReadByte(); // +4
            Debug.Assert(Unk4 == 255 
                || Unk4 == 6 
                || Unk4 == 1 
                || Unk4 == 106
                || Unk4 == 0
                || Unk4 == 2
                || Unk4 == 3
                );
            Unk5 = br.ReadByte(); // +5
            MapId = (MapDataId)br.ReadUInt16(); // +6
            Unk8 = br.ReadUInt16(); // +8
        }

        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Direction { get; set; }
        public MapDataId MapId { get; set; } // 0 = stay on current map

        public byte Unk4 { get; set; } // 255 on 2D maps, (1,6) on 3D maps
        public byte Unk5 { get; set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"teleport {MapId} <{X}, {Y}> Dir:{Direction} ({Unk4} {Unk5} {Unk8})";
    }
}