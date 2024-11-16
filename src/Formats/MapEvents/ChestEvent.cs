using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("chest", "Opens the inventory screen for the given chest")]
public class ChestEvent : MapEvent, ILockedInventoryEvent
{
    ChestEvent() { }
    public ChestEvent(ChestId chestId, ItemId key, byte difficulty, byte openedText, byte unlockedText)
    {
        ChestId = chestId;
        Key = key;
        PickDifficulty = difficulty;
        OpenedText = openedText;
        UnlockedText = unlockedText;
    }

    public static ChestEvent Serdes(ChestEvent e, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new ChestEvent();
        e.PickDifficulty = s.UInt8(nameof(PickDifficulty), e.PickDifficulty);
        e.Key = ItemId.SerdesU16(nameof(Key), e.Key, AssetType.Item, mapping, s);
        e.UnlockedText = s.UInt8(nameof(UnlockedText), e.UnlockedText);
        e.OpenedText = s.UInt8(nameof(OpenedText), e.OpenedText);
        e.ChestId = ChestId.SerdesU16(nameof(ChestId), e.ChestId, mapping, s);
        return e;
    }

    public override MapEventType EventType => MapEventType.Chest;
    [EventPart("id")] public ChestId ChestId { get; private set; }
    [EventPart("key_id", true, "None")] public ItemId Key { get; private set; }
    [EventPart("difficulty", true, (byte)0)] public byte PickDifficulty { get; private set; }
    [EventPart("open_text", true, (byte)255)] public byte OpenedText { get; private set; }
    [EventPart("unlock_text", true, (byte)255)] public byte UnlockedText { get; private set; }
}