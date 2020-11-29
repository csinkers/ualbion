using System;
using System.Globalization;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    public class DataChangeEvent : MapEvent
    {
        public static DataChangeEvent Serdes(DataChangeEvent e, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new DataChangeEvent();
            e.Property = s.EnumU8(nameof(Property), e.Property);
            e.Mode = s.EnumU8(nameof(Mode), e.Mode);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.PartyMemberId = PartyMemberId.SerdesU8(nameof(PartyMemberId), e.PartyMemberId, mapping, s);

            switch (e.Property)
            {
                case ChangeProperty.ReceiveOrRemoveItem:
                    e._value = ItemId.SerdesU16(nameof(ItemId), ItemId.FromUInt32(e._value), AssetType.Item, mapping, s).ToUInt32();
                    break;
                default:
                    e._value = s.UInt16(nameof(Value), (ushort)e._value);
                    break;
            }
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

        uint _value;

        public ChangeProperty Property { get; private set; }
        public QuantityChangeOperation Mode { get; private set;  } // No mode for adding XP
        public PartyMemberId PartyMemberId { get; private set;  }
        public ushort Amount { get; private set;  } // Or language id

        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }

        public ushort Value => (ushort)_value;
        public ItemId ItemId => Property == ChangeProperty.ReceiveOrRemoveItem
            ? ItemId.FromUInt32(_value)
            : throw new InvalidOperationException("Tried to retrieve the ItemId of a non-item DataChangeEvent");

        public PlayerCondition Status => Property == ChangeProperty.Status 
            ? (PlayerCondition)_value 
            : throw new InvalidOperationException("Tried to retrieve the Status of a non-status DataChangeEvent");

        public PlayerLanguages Language => Property == ChangeProperty.AddLanguage 
            ? (PlayerLanguages)_value 
            : throw new InvalidOperationException("Tried to retrieve the Language of a non-language DataChangeEvent");

        string ItemString =>
            Property switch
            {
                ChangeProperty.Status => Status.ToString(),
                ChangeProperty.AddLanguage => Language.ToString(),
                ChangeProperty.ReceiveOrRemoveItem => ItemId.ToString(),
                _ => Value.ToString(CultureInfo.InvariantCulture)
            };
        public override string ToString() => $"data_change {PartyMemberId.ToString() ?? "ActivePlayer"} {Property} {Mode} {Amount}x{ItemString} ({Unk3} {Unk4})";
        public override MapEventType EventType => MapEventType.DataChange;
    }
}
