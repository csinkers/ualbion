using System.Diagnostics;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class DataChangeEvent : MapEvent
    {
        public enum ChangeProperty : byte
        {
            Health              = 0x0, // Maybe 1?
            MagicPoints         = 0x1, // Maybe 2?
            Status              = 0x5,
            AddLanguage         = 0x7,
            AddExperience       = 0x8,
            ReceiveOrRemoveItem = 0x13,
            Gold                = 0x14,
            Food                = 0x15
        }

        public enum ChangeMode : byte
        {
            SetToMinimum       = 0,
            SetToMaximum       = 1,
            Unk2               = 2,
            SetAmount          = 3,
            AddAmount          = 4,
            SubtractAmount     = 5,
            AddPercentage      = 6,
            SubtractPercentage = 7
        }

        public enum PlayerStatus
        {
            Unconscious = 0,
            Poisoned    = 1,
            Ill         = 2,
            Exhausted   = 3,
            Paralysed   = 4,
            Fleeing     = 5,
            Intoxicated = 6,
            Blind       = 7,
            Panicking   = 8,
            Asleep      = 9,
            Insane      = 10,
            Irritated   = 11
        }

        public override EventType Type => EventType.DataChange;
        public ChangeProperty Property { get; set; }
        public ChangeMode Mode { get; set;  } // No mode for adding XP
        public byte AlwaysOne { get => 1; set => Debug.Assert(value == 1); }
        public byte AlwaysZero { get => 0; set => Debug.Assert(value == 0); }
        public byte PartyMemberId { get; set;  }
        public ushort Amount { get; set;  } // Or language id

        public DataChangeEvent(BinaryReader br, int id)
        {
            Property = (ChangeProperty)br.ReadByte();
            Mode = (ChangeMode) br.ReadByte();
            AlwaysOne = br.ReadByte();
            AlwaysZero = br.ReadByte();
            PartyMemberId = br.ReadByte();
            Amount = br.ReadUInt16();
        }
    }
}