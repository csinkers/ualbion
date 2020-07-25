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
                DoorId = ushort.Parse(args[1]),
                PickDifficulty = args.Length > 2 ? byte.Parse(args[2]) : (byte)0,
                InitialTextId =  args.Length > 3 ? byte.Parse(args[3]) : (byte)255,
                UnlockedTextId = args.Length > 4 ? byte.Parse(args[4]) : (byte)255,
                KeyItemId = args.Length > 5 ? (ItemId?)int.Parse(args[5]) : null,
            };
        }

        public static DoorEvent Serdes(DoorEvent e, ISerializer s, AssetType textType, ushort textSourceId)
        {
            e ??= new DoorEvent(textType, textSourceId);
            s.Begin();
            e.PickDifficulty = s.UInt8(nameof(PickDifficulty), e.PickDifficulty);
            e.KeyItemId = s.TransformEnumU16(nameof(KeyItemId), e.KeyItemId, StoreIncrementedNullZero<ItemId>.Instance);
            e.InitialTextId = s.UInt8(nameof(InitialTextId), e.InitialTextId);
            e.UnlockedTextId = s.UInt8(nameof(UnlockedTextId), e.UnlockedTextId);
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
        public ushort Submode => DoorId;
        public override string ToString() => $"inv:door {DoorId} {PickDifficulty}% Initial:{InitialTextId} Unlocked:{UnlockedTextId} Key:{KeyItemId}";
        public override MapEventType EventType => MapEventType.Door;
        public AssetType TextType { get; }
        public ushort TextSourceId { get; }
        public InventoryMode Mode => InventoryMode.LockedDoor;
    }
}
