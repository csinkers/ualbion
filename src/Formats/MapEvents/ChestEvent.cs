using System;
using System.Globalization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("inv:chest", "Opens the inventory screen for the given chest")]
    public class ChestEvent : MapEvent, ILockedInventoryEvent
    {
        public static ChestEvent Parse(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            return new ChestEvent(TextId.None)
            {
                ChestId = ChestId.Parse(args[1]),
                PickDifficulty = args.Length > 2 ? byte.Parse(args[2], CultureInfo.InvariantCulture) : (byte)0,
                InitialTextId =  args.Length > 3 ? byte.Parse(args[3], CultureInfo.InvariantCulture) : (byte)255,
                UnlockedTextId = args.Length > 4 ? byte.Parse(args[4], CultureInfo.InvariantCulture) : (byte)255,
                KeyItemId = args.Length > 5 ? ItemId.Parse(args[5]) : ItemId.None,
            };
        }

        ChestEvent(TextId textSourceId)
        {
            TextSourceId = textSourceId;
        }

        public static ChestEvent Serdes(ChestEvent e, AssetMapping mapping, ISerializer s, TextId textSourceId)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new ChestEvent(textSourceId);
            e.PickDifficulty = s.UInt8(nameof(PickDifficulty), e.PickDifficulty);
            e.KeyItemId = ItemId.SerdesU16(nameof(KeyItemId), e.KeyItemId, AssetType.Item, mapping, s);
            e.UnlockedTextId = s.UInt8(nameof(UnlockedTextId), e.UnlockedTextId);
            e.InitialTextId = s.UInt8(nameof(InitialTextId), e.InitialTextId);
            e.ChestId = ChestId.SerdesU16(nameof(ChestId), e.ChestId, mapping, s);
            return e;
        }

        public override MapEventType EventType => MapEventType.Chest;
        public ChestId ChestId { get; private set; }
        public byte PickDifficulty { get; private set; }
        public ItemId KeyItemId { get; private set; }
        public byte InitialTextId { get; private set; }
        public byte UnlockedTextId { get; private set; }
        public TextId TextSourceId { get; }
        public override string ToString() => $"inv:chest {ChestId} {PickDifficulty}% Initial:{InitialTextId} Unlocked:{UnlockedTextId} Key:{KeyItemId}";
    }
}
