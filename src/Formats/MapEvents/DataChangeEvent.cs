using System;
using System.Globalization;
using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    public class DataChangeEvent : MapEvent
    {
        public static DataChangeEvent Serdes(DataChangeEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new DataChangeEvent();
            s.Begin();
            e.Property = s.EnumU8(nameof(Property), e.Property);
            e.Mode = s.EnumU8(nameof(Mode), e.Mode);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.PartyMemberId = s.TransformEnumU8(nameof(PartyMemberId), e.PartyMemberId, StoreIncrementedNullZero<PartyCharacterId>.Instance);
            e.Value = s.UInt16(nameof(Value), e.Value);
            e.Amount = s.UInt16(nameof(Amount), e.Amount);
            s.End();
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

        public ChangeProperty Property { get; private set; }
        public QuantityChangeOperation Mode { get; private set;  } // No mode for adding XP
        public PartyCharacterId? PartyMemberId { get; private set;  }
        public ushort Amount { get; private set;  } // Or language id

        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public ushort Value { get; private set; }
        public ItemId ItemId => (ItemId)Value - 1;

        string ItemString =>
            Property switch
            {
                ChangeProperty.Status => ((PlayerStatus)Value).ToString(),
                ChangeProperty.AddLanguage => ((PlayerLanguages)Value).ToString(),
                ChangeProperty.ReceiveOrRemoveItem => ((ItemId)Value-1).ToString(),
                _ => Value.ToString(CultureInfo.InvariantCulture)
            };
        public override string ToString() => $"data_change {PartyMemberId?.ToString() ?? "ActivePlayer"} {Property} {Mode} {Amount}x{ItemString} ({Unk3} {Unk4})";
        public override MapEventType EventType => MapEventType.DataChange;
    }
}
