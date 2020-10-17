using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("teleport", "teleports the party to a specific location")]
    public class TeleportEvent : MapEvent
    {
        public static TeleportEvent Serdes(TeleportEvent e, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new TeleportEvent();
            e.X = s.UInt8(nameof(X), e.X);
            e.Y = s.UInt8(nameof(Y), e.Y);
            e.Direction = s.EnumU8(nameof(Direction), e.Direction);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.MapId = MapId.SerdesU8(nameof(MapId), e.MapId, mapping, s);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            ApiUtil.Assert(e.Unk4 == 0
                         || e.Unk4 == 1
                         || e.Unk4 == 2
                         || e.Unk4 == 3
                         || e.Unk4 == 6
                         || e.Unk4 == 106
                         || e.Unk4 == 255); // Always 255 in maps
            ApiUtil.Assert(e.Unk8 == 0);
            return e;
        }

        TeleportEvent() { }
        public TeleportEvent(MapId mapId, byte x, byte y)
        {
            MapId = mapId;
            X = x;
            Y = y;
        }

        [EventPart("map")] public MapId MapId { get; private set; } // 0 = stay on current map
        [EventPart("x")] public byte X { get; private set; }
        [EventPart("y")] public byte Y { get; private set; }
        public TeleportDirection Direction { get; private set; }

        public byte Unk4 { get; private set; } // 255 on 2D maps, (1,6) on 3D maps
        public byte Unk5 { get; private set; } // 2,3,4,5,6,8,9
        ushort Unk8 { get; set; }
        public override string ToString() => $"teleport {MapId} <{X}, {Y}> Dir:{Direction} ({Unk4} {Unk5})";
        public override MapEventType EventType => MapEventType.MapExit;
    }
}
