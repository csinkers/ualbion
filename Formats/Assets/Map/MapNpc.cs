using System;
using System.Linq;
using SerdesNet;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Map
{
    public class MapNpc
    {
        public const int SizeOnDisk = 10;

        [Flags]
        public enum NpcFlags : byte
        {
            Unk0 = 1,
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
            RandomMask = 3,
            Random1 = 1,
            Random2 = 2,
            FollowParty = 4,
            Stationary = 8,
        }

        public struct Waypoint
        {
            public byte X;
            public byte Y;
            public override string ToString() => $"({X}, {Y})";
        }

        public byte? Id { get; set; }
        // public SampleId? Sound { get; set; }
        public byte Sound { get; set; }
        public ushort ObjectNumber { get; set; }
        public NpcFlags Flags { get; set; } // 1=Dialogue, 2=AutoAttack, 11=ReturnMsg
        public MovementType Movement { get; set; }
        public byte Unk8 { get; set; }
        public byte Unk9 { get; set; }
        public Waypoint[] Waypoints { get; set; }
        public EventChain Chain { get; set; }
        public IEventNode Node { get; set; }

        public static MapNpc Serdes(int _, MapNpc existing, ISerializer s)
        {
            var npc = existing ?? new MapNpc();
            s.Begin();
            npc.Id = s.Transform<byte, byte?>(nameof(Id), npc.Id, s.UInt8, Tweak.Instance);
            // npc.Sound = (SampleId?)Tweak.Serdes(nameof(Sound), (byte?)npc.Sound, s.UInt8);
            npc.Sound = s.UInt8(nameof(Sound), npc.Sound);

            ushort? eventNumber = ConvertMaxToNull.Serdes(nameof(npc.Node), npc.Node?.Id, s.UInt16);
            if(eventNumber != null && npc.Node == null)
                npc.Node = new DummyEventNode(eventNumber.Value);

            npc.ObjectNumber = s.Transform<ushort, ushort?>(nameof(ObjectNumber), npc.ObjectNumber, s.UInt16, Tweak.Instance) ?? 0;
            npc.Flags = s.EnumU8(nameof(Flags), npc.Flags);
            npc.Movement = s.EnumU8(nameof(Movement), npc.Movement);
            npc.Unk8 = s.UInt8(nameof(Unk8), npc.Unk8);
            npc.Unk9 = s.UInt8(nameof(Unk9), npc.Unk9);
            s.End();
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

        public void Unswizzle(Func<ushort, (EventChain, IEventNode)> getEvent)
        {
            if (Node is DummyEventNode dummy)
                (Chain, Node) = getEvent(dummy.Id);
        }

        public override string ToString() => $"Npc{(int)Id} {Id} Obj{ObjectNumber} F:{Flags:x} M{Movement} Amount:{Unk8} Unk9:{Unk9} S{Sound} E{Node?.Id}";
    }
}
