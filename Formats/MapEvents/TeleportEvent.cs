using System.Diagnostics;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class TeleportEvent : IMapEvent
    {
        public static TeleportEvent Translate(TeleportEvent e, ISerializer s)
        {
            e ??= new TeleportEvent();
            s.Dynamic(e, nameof(X));
            s.Dynamic(e, nameof(Y));
            s.Dynamic(e, nameof(Direction));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.UInt16(nameof(MapId),
                () => (ushort)e.MapId,
                x => e.MapId = (MapDataId)x);
            s.Dynamic(e, nameof(Unk8));
            Debug.Assert(e.Unk4 == 0
                         || e.Unk4 == 1
                         || e.Unk4 == 2
                         || e.Unk4 == 3
                         || e.Unk4 == 6
                         || e.Unk4 == 106
                         || e.Unk4 == 255); // Always 255 in maps
            Debug.Assert(e.Unk8 == 0);
            return e;
        }

        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Direction { get; set; } // 0,1,2,3,255
        public MapDataId MapId { get; set; } // 0 = stay on current map

        public byte Unk4 { get; set; } // 255 on 2D maps, (1,6) on 3D maps
        public byte Unk5 { get; set; } // 2,3,4,5,6,8,9
        ushort Unk8 { get; set; } 
        public override string ToString() => $"teleport {MapId} <{X}, {Y}> Dir:{Direction} ({Unk4} {Unk5})";
        public MapEventType EventType => MapEventType.MapExit;
    }
}
