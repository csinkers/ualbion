using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class DataChangeEvent : IMapEvent
    {
        public static DataChangeEvent Translate(DataChangeEvent node, ISerializer s)
        {
            node ??= new DataChangeEvent();
            s.EnumU8(nameof(Property), () => node.Property, x => node.Property = x, x => ((byte)x, x.ToString()));
            s.EnumU8(nameof(Mode), () => node.Mode, x => node.Mode = x, x => ((byte)x, x.ToString()));
            s.Dynamic(node, nameof(Unk3));
            s.Dynamic(node, nameof(Unk4));
            s.UInt8(nameof(PartyMemberId),
                () => (byte)node.PartyMemberId,
                x => node.PartyMemberId = (PartyCharacterId)x);
            s.Dynamic(node, nameof(Value));
            s.Dynamic(node, nameof(Amount));
            return node;
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
        public MapEventType EventType => MapEventType.DataChange;
    }
}
