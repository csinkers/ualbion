using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("inv:chest", "Opens the inventory screen for the given chest")]
    public class ChestEvent : MapEvent, ILockedInventoryEvent
    {
        public static ChestEvent Parse(string[] args)
        {
            return new ChestEvent(AssetType.SystemText, 0)
            {
                ChestId = (ChestId)int.Parse(args[0]),
                PickDifficulty = byte.Parse(args[1]),
                Member = args.Length > 2 ? (PartyCharacterId?)int.Parse(args[2]) : null,
            };
        }

        ChestEvent(AssetType textType, ushort textSourceId)
        {
            TextType = textType;
            TextSourceId = textSourceId;
        }

        public static ChestEvent Serdes(ChestEvent e, ISerializer s, AssetType textType, ushort textSourceId)
        {
            e ??= new ChestEvent(textType, textSourceId);
            e.PickDifficulty = s.UInt8(nameof(PickDifficulty), e.PickDifficulty);
            e.KeyItemId = (ItemId?)StoreIncrementedNullZero.Serdes(nameof(KeyItemId), (ushort?)e.KeyItemId, s.UInt16);
            e.InitialTextId = s.UInt8(nameof(InitialTextId), e.InitialTextId);
            e.UnlockedTextId = s.UInt8(nameof(UnlockedTextId), e.UnlockedTextId);
            e.ChestId = s.EnumU16(nameof(ChestId), e.ChestId);
            return e;
        }

        public override MapEventType EventType => MapEventType.Chest;
        public InventoryMode Mode => InventoryMode.Chest;
        public ChestId ChestId { get; private set; }
        public byte PickDifficulty { get; private set; }
        public ItemId? KeyItemId { get; private set; }
        public byte InitialTextId { get; private set; }
        public byte UnlockedTextId { get; private set; }
        public AssetType TextType { get; }
        public ushort TextSourceId { get; }
        public PartyCharacterId? Member { get; private set; }
        public override string ToString() => $"inv:chest {ChestId} {PickDifficulty}% Key:{KeyItemId} Initial:{InitialTextId} Opened:{UnlockedTextId}";

        public ISetInventoryModeEvent CloneForMember(PartyCharacterId member)
            => new ChestEvent(TextType, TextSourceId)
            {
                ChestId = ChestId,
                KeyItemId = KeyItemId,
                PickDifficulty = PickDifficulty,
                UnlockedTextId = UnlockedTextId,
                InitialTextId = InitialTextId,
                Member = member
            };
    }
}
