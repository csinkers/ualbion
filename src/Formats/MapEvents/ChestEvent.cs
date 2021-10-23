using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("open_chest", "Opens the inventory screen for the given chest")]
    public class ChestEvent : MapEvent, ILockedInventoryEvent
    {
        ChestEvent(TextId textSource) => TextSource = textSource;
        public ChestEvent(ChestId chestId, TextId textSource, ItemId key, byte difficulty, byte openedText, byte unlockedText)
        {
            ChestId = chestId;
            TextSource = textSource;
            Key = key;
            PickDifficulty = difficulty;
            OpenedText = openedText;
            UnlockedText = unlockedText;
        }

        public static ChestEvent Serdes(ChestEvent e, AssetMapping mapping, ISerializer s, TextId textSourceId)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new ChestEvent(textSourceId);
            e.PickDifficulty = s.UInt8(nameof(PickDifficulty), e.PickDifficulty);
            e.Key = ItemId.SerdesU16(nameof(Key), e.Key, AssetType.Item, mapping, s);
            e.UnlockedText = s.UInt8(nameof(UnlockedText), e.UnlockedText);
            e.OpenedText = s.UInt8(nameof(OpenedText), e.OpenedText);
            e.ChestId = ChestId.SerdesU16(nameof(ChestId), e.ChestId, mapping, s);
            return e;
        }

        public override MapEventType EventType => MapEventType.Chest;
        [EventPart("id")] public ChestId ChestId { get; private set; }
        [EventPart("text_src")] public TextId TextSource { get; }
        [EventPart("key_id")] public ItemId Key { get; private set; }
        [EventPart("difficulty", true, "0")] public byte PickDifficulty { get; private set; }
        [EventPart("open_text", true, "255")] public byte OpenedText { get; private set; }
        [EventPart("unlock_text", true, "255")] public byte UnlockedText { get; private set; }
    }
}
