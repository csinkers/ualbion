using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("door", "Opens the inventory screen for the given door")]
public class DoorEvent : MapEvent, ILockedInventoryEvent
{
    public static DoorEvent Serdes(DoorEvent e, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new DoorEvent();
        e.PickDifficulty = s.UInt8(nameof(PickDifficulty), e.PickDifficulty);
        e.Key = ItemId.SerdesU16(nameof(Key), e.Key, AssetType.Item, mapping, s);
        e.OpenedText = s.UInt8(nameof(OpenedText), e.OpenedText);
        e.UnlockedText = s.UInt8(nameof(UnlockedText), e.UnlockedText);
        e.DoorId = DoorId.SerdesU16(nameof(DoorId), e.DoorId, mapping, s); // Usually 100+
        return e;
    }

    DoorEvent() { }
    public DoorEvent(DoorId doorId, ItemId key, byte difficulty, byte openedText, byte unlockedText)
    {
        DoorId = doorId;
        Key = key;
        PickDifficulty = difficulty;
        OpenedText = openedText;
        UnlockedText = unlockedText;
    }

    [EventPart("id")] public DoorId DoorId { get; private set; }
    [EventPart("key_id", true, "None")] public ItemId Key { get; private set; }
    [EventPart("difficulty", true, (byte)0)] public byte PickDifficulty { get; private set; }
    [EventPart("open_text", true, (byte)255)] public byte OpenedText { get; private set; }
    [EventPart("unlock_text", true, (byte)255)] public byte UnlockedText { get; private set; }
    public override MapEventType EventType => MapEventType.Door;
}