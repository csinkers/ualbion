using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("inv:door", "Opens the inventory screen for the given door")]
    public class DoorEvent : MapEvent, ILockedInventoryEvent
    {
        public static DoorEvent Serdes(DoorEvent e, AssetMapping mapping, ISerializer s, TextId textSourceId)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new DoorEvent(textSourceId);
            e.PickDifficulty = s.UInt8(nameof(PickDifficulty), e.PickDifficulty);
            e.Key = ItemId.SerdesU16(nameof(Key), e.Key, AssetType.Item, mapping, s);
            e.OpenedText = s.UInt8(nameof(OpenedText), e.OpenedText);
            e.UnlockedText = s.UInt8(nameof(UnlockedText), e.UnlockedText);
            e.DoorId = DoorId.SerdesU16(nameof(DoorId), e.DoorId, mapping, s); // Usually 100+
            return e;
        }

        DoorEvent(TextId textSourceId) => TextSource = textSourceId;
        public DoorEvent(DoorId doorId, TextId textSource, ItemId key, byte difficulty, byte openedText, byte unlockedText)
        {
            DoorId = doorId;
            TextSource = textSource;
            Key = key;
            PickDifficulty = difficulty;
            OpenedText = openedText;
            UnlockedText = unlockedText;
        }

        [EventPart("id")] public DoorId DoorId { get; private set; }
        [EventPart("text_src")] public TextId TextSource { get; }
        [EventPart("key_id")] public ItemId Key { get; private set; }
        [EventPart("difficulty", true, "0")] public byte PickDifficulty { get; private set; }
        [EventPart("open_text", true, "255")] public byte OpenedText { get; private set; }
        [EventPart("unlock_text", true, "255")] public byte UnlockedText { get; private set; }
        public override MapEventType EventType => MapEventType.Door;
    }
}
