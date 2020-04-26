using SerdesNet;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class DoorEvent : MapEvent
    {
        public static DoorEvent Serdes(DoorEvent e, ISerializer s)
        {
            e ??= new DoorEvent();
            e.PickDifficulty = s.UInt8(nameof(PickDifficulty), e.PickDifficulty);
            e.KeyItemId = (ItemId?)StoreIncrementedNullZero.Serdes(nameof(KeyItemId), (ushort?)e.KeyItemId, s.UInt16);
            e.InitialTextId = s.UInt8(nameof(InitialTextId), e.InitialTextId);
            e.UnlockedTextId = s.UInt8(nameof(UnlockedTextId), e.UnlockedTextId);
            e.DoorId = s.UInt16(nameof(DoorId), e.DoorId); // Usually 100+
            e.NextEventId = s.UInt16(nameof(NextEventId), e.NextEventId); // EventId when failed to pick??
            return e;
        }

        public byte PickDifficulty { get; private set; }
        public ItemId? KeyItemId { get; private set; }
        public byte InitialTextId { get; private set; }
        public byte UnlockedTextId { get; private set; }
        public ushort DoorId { get; private set; }
        public ushort NextEventId { get; private set; }
        public override string ToString() => $"door ({PickDifficulty} {KeyItemId} {InitialTextId} {UnlockedTextId} {DoorId} {NextEventId})";
        public override MapEventType EventType => MapEventType.Door;
    }
}
