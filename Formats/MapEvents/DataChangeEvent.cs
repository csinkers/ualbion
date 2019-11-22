using System.Diagnostics;
using System.IO;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    public class DataChangeEvent : IEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type)
        {
            var e = new DataChangeEvent
            {
                Property = (ChangeProperty) br.ReadByte(), // 1
                Mode = (QuantityChangeOperation) br.ReadByte(), // 2
                Unk3 = br.ReadByte(), // 3
                Unk4 = br.ReadByte(), // 4
                PartyMemberId = (PartyCharacterId) br.ReadByte(), // 5
                Value = br.ReadUInt16(), // 8
                Amount = br.ReadUInt16(), // 8
            };
            Debug.Assert(e.Unk4 == 0 || e.Unk4 == 3); // Always 0 for 2D?
            return new EventNode(id, e);
        }

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

        public ChangeProperty Property { get; set; }
        public QuantityChangeOperation Mode { get; set;  } // No mode for adding XP
        public PartyCharacterId PartyMemberId { get; set;  }
        public ushort Amount { get; set;  } // Or language id

        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public ushort Value { get; set; }

        string ItemString =>
            Property switch
            {
                ChangeProperty.Status => ((PlayerStatus)Value).ToString(),
                ChangeProperty.AddLanguage => ((PlayerLanguage)Value).ToString(),
                ChangeProperty.ReceiveOrRemoveItem => ((ItemId)Value-1).ToString(),
                _ => Value.ToString()
            };
        public override string ToString() => $"data_change {PartyMemberId} {Property} {Mode} {Amount}x{ItemString} ({Unk3} {Unk4})";
    }
}
