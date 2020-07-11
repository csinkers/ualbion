using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("inv:door", "Opens the inventory screen for the given door")]
    public class DoorEvent : MapEvent, ILockedInventoryEvent
    {
        public static DoorEvent Parse(string[] args)
        {
            return new DoorEvent(AssetType.SystemText, 0)
            {
                DoorId = ushort.Parse(args[0]),
                PickDifficulty = byte.Parse(args[1]),
                Member = args.Length > 2 ? (PartyCharacterId?)int.Parse(args[2]) : null,
            };
        }

        public static DoorEvent Serdes(DoorEvent e, ISerializer s, AssetType textType, ushort textSourceId)
        {
            e ??= new DoorEvent(textType, textSourceId);
            s.Begin();
            e.PickDifficulty = s.UInt8(nameof(PickDifficulty), e.PickDifficulty);
            e.KeyItemId = s.TransformEnumU16(nameof(KeyItemId), e.KeyItemId, StoreIncrementedNullZero<ItemId>.Instance);
            e.UnlockedTextId = s.UInt8(nameof(UnlockedTextId), e.UnlockedTextId);
            e.InitialTextId = s.UInt8(nameof(InitialTextId), e.InitialTextId);
            e.DoorId = s.UInt16(nameof(DoorId), e.DoorId); // Usually 100+
            s.End();
            return e;
        }

        DoorEvent(AssetType textType, ushort textSourceId)
        {
            TextType = textType;
            TextSourceId = textSourceId;
        }

        public byte PickDifficulty { get; private set; }
        public ItemId? KeyItemId { get; private set; }
        public byte InitialTextId { get; private set; }
        public byte UnlockedTextId { get; private set; }
        public ushort DoorId { get; private set; }
        public override string ToString() => $"inv:door {DoorId} ({PickDifficulty}% {KeyItemId} Initial:{InitialTextId} Unlocked:{UnlockedTextId})";
        public override MapEventType EventType => MapEventType.Door;
        public AssetType TextType { get; }
        public ushort TextSourceId { get; }
        public InventoryMode Mode => InventoryMode.LockedDoor;
        public PartyCharacterId? Member { get; private set; }
        public ISetInventoryModeEvent CloneForMember(PartyCharacterId member) 
            => new DoorEvent(TextType, TextSourceId)
            {
                DoorId = DoorId,
                KeyItemId = KeyItemId,
                PickDifficulty = PickDifficulty,
                InitialTextId = InitialTextId,
                UnlockedTextId = UnlockedTextId,
                Member = member
            };
    }
}
