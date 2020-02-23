using System;
using System.IO;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets
{
    public class MapNpc
    {
        public const int SizeOnDisk = 10;

        [Flags]
        public enum NpcFlags : byte
        {
            Unk0 = 1 << 0,
            Unk1 = 1 << 1,
            Unk2 = 1 << 2,
            Unk3 = 1 << 3,
            Unk4 = 1 << 4,
            Unk5 = 1 << 5,
            Unk6 = 1 << 6,
            Unk7 = 1 << 7,
        }

        [Flags]
        public enum MovementType : byte
        {
            Random1 = 1,
            Random2 = 2,
            FollowParty = 4,
            Stationary = 8,

            RandomMask = 3,
        }

        public struct Waypoint
        {
            public byte X;
            public byte Y;
            public override string ToString() => $"({X}, {Y})";
        }

        public NpcCharacterId Id { get; set; }
        public byte Sound { get; set; }
        public ushort? EventNumber { get; set; }
        public ushort ObjectNumber { get; set; }
        public NpcFlags Flags { get; set; } // 1=Dialogue, 2=AutoAttack, 11=ReturnMsg
        public MovementType Movement { get; set; }
        public byte Unk8 { get; set; }
        public byte Unk9 { get; set; }
        public Waypoint[] Waypoints { get; set; }
        public IEventNode EventChain { get; set; }

        public static MapNpc Serdes(int _, MapNpc existing, ISerializer s)
        {
            var npc = existing ?? new MapNpc();
            npc.Id = s.EnumU8(nameof(Id), npc.Id);
            npc.Sound = s.UInt8(nameof(Sound), npc.Sound);
            npc.EventNumber = ConvertMaxToNull.Serdes(nameof(EventNumber), npc.EventNumber, s.UInt16);
            npc.ObjectNumber = Tweak.Serdes(nameof(ObjectNumber), npc.ObjectNumber, s.UInt16) ?? 0;
            npc.Flags = s.EnumU8(nameof(Flags), npc.Flags);
            npc.Movement = s.EnumU8(nameof(Movement), npc.Movement);
            npc.Unk8 = s.UInt8(nameof(Unk8), npc.Unk8);
            npc.Unk9 = s.UInt8(nameof(Unk9), npc.Unk9);
            return npc;
        }

        public static MapNpc Load(BinaryReader br)
        {
            var npc = new MapNpc();
            npc.Id = (NpcCharacterId)br.ReadByte(); // +0
            npc.Sound = br.ReadByte(); // +1
            npc.EventNumber = br.ReadUInt16(); // +2
            if (npc.EventNumber == 0xffff) npc.EventNumber = null;

            npc.ObjectNumber = FormatUtil.Tweak(br.ReadUInt16()) ?? 0; // +4
            npc.Flags = (NpcFlags)br.ReadByte(); // +6 // Combine this & MovementType ?
            npc.Movement = (MovementType)br.ReadByte(); // +7
            npc.Unk8 = br.ReadByte(); // +8
            npc.Unk9 = br.ReadByte(); // +9
            return npc;
        }

        public void LoadWaypoints(ISerializer s)
        {
            if ((Movement & MovementType.RandomMask) != 0)
            {
                var wp = Waypoints?.FirstOrDefault() ?? new Waypoint();
                wp.X = s.UInt8("X", wp.X);
                wp.Y = s.UInt8("Y", wp.Y);
                Waypoints = new[] { wp };
            }
            else
            {
                Waypoints ??= new Waypoint[0x480];
                for (int i = 0; i < Waypoints.Length; i++)
                {
                    Waypoints[i].X = s.UInt8(null, Waypoints[i].X);
                    Waypoints[i].Y = s.UInt8(null, Waypoints[i].Y);
                }
            }
        }

        public void LoadWaypoints(BinaryReader br)
        {
            if ((Movement & MovementType.RandomMask) != 0)
            {
                var wp = new Waypoint();
                wp.X = br.ReadByte();
                wp.Y = br.ReadByte();
                Waypoints = new[] { wp };
            }
            else
            {
                Waypoints = new Waypoint[0x480];
                for (int i = 0; i < Waypoints.Length; i++)
                {
                    Waypoints[i].X = br.ReadByte();
                    Waypoints[i].Y = br.ReadByte();
                }
            }
        }

        public override string ToString() => $"Npc{(int)Id} {Id} Obj{ObjectNumber} F:{Flags:x} M{Movement} Amount:{Unk8} Unk9:{Unk9} S{Sound} E{EventNumber}";
    }
}