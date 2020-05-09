using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    public class DataChangeEvent : MapEvent
    {
        public static DataChangeEvent Serdes(DataChangeEvent e, ISerializer s)
        {
            e ??= new DataChangeEvent();
            e.Property = s.EnumU8(nameof(Property), e.Property);
            e.Mode = s.EnumU8(nameof(Mode), e.Mode);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.PartyMemberId = (PartyCharacterId?)StoreIncrementedNullZero.Serdes(nameof(PartyMemberId), (byte?)e.PartyMemberId, s.UInt8);
            e.Value = s.UInt16(nameof(Value), e.Value);
            e.Amount = s.UInt16(nameof(Amount), e.Amount);
            return e;
        }

        public enum ChangeProperty : byte
        {
            Health              = 0x2, // Maybe 1?
            MagicPoints         = 0x3, // Maybe 2?
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
        public PartyCharacterId? PartyMemberId { get; set;  }
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
        public override string ToString() => $"data_change {PartyMemberId?.ToString() ?? "ActivePlayer"} {Property} {Mode} {Amount}x{ItemString} ({Unk3} {Unk4})";
        public override MapEventType EventType => MapEventType.DataChange;
    }
}
