using System;
using SerdesNet;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Maps
{
    public class MapNpc
    {
        public const int SizeOnDisk = 10;

        [Flags]
        public enum NpcFlags : byte
        {
            NonPartySeeking = 1,
            IsMonster = 1 << 1,
            Unk2 = 1 << 2,
            Unk3 = 1 << 3, // Has contact event?
            Unk4 = 1 << 4,
            Unk5 = 1 << 5,
            Unk6 = 1 << 6,
            Unk7 = 1 << 7,
        }

        [Flags]
        public enum MovementTypes : byte
        {
            RandomMask = 3,
            Random1 = 1,
            Random2 = 2,
            Unk4 = 4,
            Stationary = 8,
        }

        public byte? Id { get; set; } // MonsterGroup, NpcCharacterId etc
        // public SampleId? Sound { get; set; }
        public byte Sound { get; set; }
        public ushort ObjectNumber { get; set; } // LargeNpcGfx, SmallNpcGfx etc
        public NpcFlags Flags { get; set; } // 1=Dialogue, 2=AutoAttack, 11=ReturnMsg
        public MovementTypes Movement { get; set; }
        public byte Unk8 { get; set; }
        public byte Unk9 { get; set; }
        public NpcWaypoint[] Waypoints { get; set; }
        public EventChain Chain { get; set; }
        public IEventNode Node { get; set; }

        public static MapNpc Serdes(int _, MapNpc existing, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var npc = existing ?? new MapNpc();
            npc.Id = s.Transform<byte, byte?>(nameof(Id), npc.Id, S.UInt8, TweakedConverter.Instance);
            // npc.Sound = (SampleId?)Tweak.Serdes(nameof(Sound), (byte?)npc.Sound, s.UInt8);
            npc.Sound = s.UInt8(nameof(Sound), npc.Sound);

            ushort? eventNumber = MaxToNullConverter.Serdes(nameof(npc.Node), npc.Node?.Id, s.UInt16);
            if (eventNumber != null && npc.Node == null)
                npc.Node = new DummyEventNode(eventNumber.Value);

            npc.ObjectNumber = s.Transform<ushort, ushort?>(nameof(ObjectNumber), npc.ObjectNumber, S.UInt16, TweakedConverter.Instance) ?? 0;
            npc.Flags = s.EnumU8(nameof(Flags), npc.Flags);
            npc.Movement = s.EnumU8(nameof(Movement), npc.Movement);
            npc.Unk8 = s.UInt8(nameof(Unk8), npc.Unk8);
            npc.Unk9 = s.UInt8(nameof(Unk9), npc.Unk9);
            return npc;
        }

        public void LoadWaypoints(ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if ((Movement & MovementTypes.RandomMask) != 0)
            {
                var wp = Waypoints?[0];
                byte x = wp?.X ?? 0;
                byte y = wp?.Y ?? 0;
                x = s.UInt8("X", x);
                y = s.UInt8("Y", y);
                Waypoints = new[] { new NpcWaypoint(x, y) };
            }
            else
            {
                Waypoints ??= new NpcWaypoint[0x480];
                for (int i = 0; i < Waypoints.Length; i++)
                {
                    byte x = s.UInt8(null, Waypoints[i].X);
                    byte y = s.UInt8(null, Waypoints[i].Y);
                    Waypoints[i] = new NpcWaypoint(x, y);
                }
            }
        }

        public void Unswizzle(Func<ushort, (EventChain, IEventNode)> getEvent)
        {
            if (getEvent == null) throw new ArgumentNullException(nameof(getEvent));
            if (Node is DummyEventNode dummy)
                (Chain, Node) = getEvent(dummy.Id);
        }

        public override string ToString() => $"Npc{(int)Id} {Id} Obj{ObjectNumber} F:{Flags:x} M{Movement} Amount:{Unk8} Unk9:{Unk9} S{Sound} E{Node?.Id}";
    }
}
