using System;
using System.Globalization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("inv:door", "Opens the inventory screen for the given door")]
    public class DoorEvent : MapEvent, ILockedInventoryEvent
    {
        public static DoorEvent Parse(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            return new DoorEvent(TextId.None)
            {
                DoorId = new DoorId(AssetType.Door, ushort.Parse(args[1], CultureInfo.InvariantCulture)),
                PickDifficulty = args.Length > 2 ? byte.Parse(args[2], CultureInfo.InvariantCulture) : (byte)0,
                InitialTextId =  args.Length > 3 ? byte.Parse(args[3], CultureInfo.InvariantCulture) : (byte)255,
                UnlockedTextId = args.Length > 4 ? byte.Parse(args[4], CultureInfo.InvariantCulture) : (byte)255,
                KeyItemId = args.Length > 5 ? new ItemId(AssetType.Item, int.Parse(args[5], CultureInfo.InvariantCulture)) : ItemId.None, // TODO Better parsing
            };
        }

        public static DoorEvent Serdes(DoorEvent e, AssetMapping mapping, ISerializer s, TextId textSourceId)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new DoorEvent(textSourceId);
            e.PickDifficulty = s.UInt8(nameof(PickDifficulty), e.PickDifficulty);
            e.KeyItemId = ItemId.SerdesU16(nameof(KeyItemId), e.KeyItemId, AssetType.Item, mapping, s);
            e.InitialTextId = s.UInt8(nameof(InitialTextId), e.InitialTextId);
            e.UnlockedTextId = s.UInt8(nameof(UnlockedTextId), e.UnlockedTextId);
            e.DoorId = DoorId.SerdesU16(nameof(DoorId), e.DoorId, mapping, s); // Usually 100+
            return e;
        }

        DoorEvent(TextId textSourceId)
        {
            TextSourceId = textSourceId;
        }

        public byte PickDifficulty { get; private set; }
        public ItemId KeyItemId { get; private set; }
        public byte InitialTextId { get; private set; }
        public byte UnlockedTextId { get; private set; }
        public DoorId DoorId { get; private set; }
        public override string ToString() => $"inv:door {DoorId} {PickDifficulty}% Initial:{InitialTextId} Unlocked:{UnlockedTextId} Key:{KeyItemId}";
        public override MapEventType EventType => MapEventType.Door;
        public TextId TextSourceId { get; }
    }
}
