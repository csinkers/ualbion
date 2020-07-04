using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets.Save
{
    public class SavedGame
    {
        public const int MaxPartySize = 6;
        public const int MaxNpcCount = 96;
        public static readonly DateTime Epoch = new DateTime(2200, 1, 1, 0, 0, 0);

        public string Name { get; set; }
        public ushort Version { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public MapDataId MapId { get; set; }
        public ushort PartyX { get; set; }
        public ushort PartyY { get; set; }
        public Direction PartyDirection { get; set; }

        public IDictionary<PartyCharacterId, CharacterSheet> PartyMembers { get; } = new Dictionary<PartyCharacterId, CharacterSheet>();
        public IDictionary<NpcCharacterId, CharacterSheet> NpcStats { get; } = new Dictionary<NpcCharacterId, CharacterSheet>();
        public IDictionary<AutoMapId, byte[]> Automaps { get; } = new Dictionary<AutoMapId, byte[]>();
        public IDictionary<ChestId, Inventory> Chests { get; } = new Dictionary<ChestId, Inventory>();
        public IDictionary<MerchantId, Inventory> Merchants { get; } = new Dictionary<MerchantId, Inventory>();
        TickerSet _tickers { get; } = new TickerSet();
        FlagSet _switches { get; } = new FlagSet();

        public IDictionary<int, byte> Tickers => _tickers;
        public IDictionary<int, bool> Switches => _switches;

        public ushort Unk0 { get; set; }
        public uint Unk1 { get; set; }
        public byte[] Unk9 { get; set; }
        public byte[] Unknown16 { get; set; }
        public byte[] Unknown1A6 { get; set; }
        public byte[] Unknown2C1 { get; set; }
        public byte[] Unknown5B9F { get; set; }
        public NpcState[] Npcs { get; } = new NpcState[MaxNpcCount];
        public byte[] Unknown5B71 { get; set; }
        public MapChangeList PermanentMapChanges { get; private set; } = new MapChangeList();
        public MapChangeList TemporaryMapChanges { get; private set; } = new MapChangeList();
        public VisitedEventList VisitedEvents { get; set; }
        public PartyCharacterId?[] ActiveMembers { get; } = new PartyCharacterId?[MaxPartySize];

        public static string GetName(BinaryReader br)
        {
            var s = new AlbionReader(br, br.BaseStream.Length);
            ushort nameLength = s.UInt16("NameLength", 0);
            if (nameLength > 1024)
                return "Invalid";

            s.UInt16(nameof(Unk0), 0);
            return s.FixedLengthString(nameof(Name), null, nameLength);
        }

        public static SavedGame Serdes(SavedGame save, ISerializer s)
        {
            save ??= new SavedGame();

            ushort nameLength = s.UInt16("NameLength", (ushort)(save.Name?.Length ?? 0));
            save.Unk0 = s.UInt16(nameof(Unk0), save.Unk0);
            save.Name = s.FixedLengthString(nameof(Name), save.Name, nameLength);

            save.Unk1 = s.UInt32(nameof(Unk1), save.Unk1);
            var versionOffset = s.Offset;
            save.Version = s.UInt16(nameof(Version), save.Version); // 0
            ApiUtil.Assert(save.Version == 138); // TODO: Throw error for other versions?
            save.Unk9 = s.ByteArray(nameof(Unk9), save.Unk9, 6); // 2

            ushort days    = s.UInt16("Days", (ushort)save.ElapsedTime.TotalDays);  // 8
            ushort hours   = s.UInt16("Hours", (ushort)save.ElapsedTime.Hours);     // A
            ushort minutes = s.UInt16("Minutes", (ushort)save.ElapsedTime.Minutes); // C
            save.ElapsedTime = new TimeSpan(days, hours, minutes, save.ElapsedTime.Seconds, save.ElapsedTime.Milliseconds);
            save.MapId   = s.EnumU16(nameof(MapId), save.MapId);    // E
            save.PartyX  = s.UInt16(nameof(PartyX), save.PartyX);   // 10
            save.PartyY  = s.UInt16(nameof(PartyY), save.PartyY);   // 12
            save.PartyDirection = s.EnumU16(nameof(PartyDirection), save.PartyDirection); // 14

            save.Unknown16 = s.ByteArrayHex(nameof(Unknown16), save.Unknown16, 0x184); // 16

            for (int i = 0; i < save.ActiveMembers.Length; i++) // 6 x PlayerId @ 19A
            {
                save.ActiveMembers[i] = (PartyCharacterId?)StoreIncrementedNullZero.Serdes(
                    nameof(save.ActiveMembers),
                    (ushort?)save.ActiveMembers[i],
                    s.UInt16);
            }

            save.Unknown1A6 = s.ByteArrayHex(nameof(Unknown1A6), save.Unknown1A6, 0xD0); // 1A6
            save._switches.Packed = s.ByteArrayHex(nameof(Switches), save._switches.Packed, FlagSet.PackedSize); // 276
            save.Unknown2C1 = s.ByteArrayHex(nameof(Unknown2C1), save.Unknown2C1, 0x5833); // 0x2C1
            s.Meta(nameof(Tickers), save._tickers.Serdes, save._tickers.Serdes); // 5AF4

            save.Unknown5B9F = s.ByteArrayHex(nameof(Unknown5B9F), save.Unknown5B9F, 0x2C);
            s.List(nameof(save.Npcs), save.Npcs, MaxNpcCount, NpcState.Serdes);

            save.Unknown5B71 = s.ByteArrayHex(
                nameof(Unknown5B71),
                save.Unknown5B71,
                (int)(0x947C + versionOffset - s.Offset)); // 5B9F

            save.PermanentMapChanges = s.Meta(nameof(PermanentMapChanges), save.PermanentMapChanges, MapChangeList.Serdes);
            save.TemporaryMapChanges = s.Meta(nameof(TemporaryMapChanges), save.TemporaryMapChanges, MapChangeList.Serdes);
            save.VisitedEvents = s.Meta(nameof(VisitedEvents), save.VisitedEvents, VisitedEventList.Serdes);

            var charLoader = new CharacterSheetLoader();
            void SerdesPartyCharacter(int i, int size, ISerializer serializer)
            {
                if (i > 0xff)
                    return;

                var key = (PartyCharacterId)i;
                CharacterSheet existing = null;
                if (size > 0 || save.PartyMembers.TryGetValue(key, out existing))
                {
                    save.PartyMembers[key] = charLoader.Serdes(
                        existing,
                        serializer,
                        key.ToAssetId(),
                        new BasicAssetInfo { Id = i });
                }
            }

        void SerdesNpcCharacter(int i, int size, ISerializer serializer)
            {
                if (i > 0xff)
                    return;

                var key = (NpcCharacterId)i;
                CharacterSheet existing = null;
                if (serializer.Mode == SerializerMode.Reading || save.NpcStats.TryGetValue(key, out existing))
                {
                    save.NpcStats[key] = charLoader.Serdes(
                        existing,
                        serializer,
                        key.ToAssetId(),
                        new BasicAssetInfo { Id = i });
                }
            }

            void SerdesAutomap(int i, int size, ISerializer serializer)
            {
                var key = (AutoMapId)i;
                if (save.Automaps.TryGetValue(key, out _))
                    serializer.ByteArray("Automap" + i, save.Automaps[key], save.Automaps[key].Length);
                else if (serializer.Mode == SerializerMode.Reading)
                    save.Automaps[key] = serializer.ByteArray("Automap" + i, null, size);
            }

            void SerdesChest(int i, int size, ISerializer serializer)
            {
                var key = (ChestId)i;
                Inventory existing = null;

                if (serializer.Mode == SerializerMode.Reading || save.Chests.TryGetValue(key, out existing))
                    save.Chests[key] = Inventory.SerdesChest(i, existing, serializer);
            }

            void SerdesMerchant(int i, int size, ISerializer serializer)
            {
                if (i > 0xff)
                    return;

                var key = (MerchantId)i;
                Inventory existing = null;

                if (serializer.Mode == SerializerMode.Reading || save.Merchants.TryGetValue(key, out existing))
                    save.Merchants[key] = Inventory.SerdesMerchant(i, existing, serializer);
            }

            var partyIds = save.PartyMembers.Keys.Select(x => (int)x).ToList();
            partyIds.Add(199); // Force extra XLD length fields to be written for empty objects to preserve compat with original game.
            partyIds.Add(299);
            XldLoader.Serdes(XldCategory.PartyCharacter, 0, s, SerdesPartyCharacter, partyIds);
            XldLoader.Serdes(XldCategory.PartyCharacter,1, s, SerdesPartyCharacter, partyIds);
            XldLoader.Serdes(XldCategory.PartyCharacter,2, s, SerdesPartyCharacter, partyIds);

            var automapIds = save.Automaps.Keys.Select(x => (int)x).ToList();
            automapIds.Add(199);
            automapIds.Add(399);
            XldLoader.Serdes(XldCategory.Automap,1, s, SerdesAutomap, automapIds);
            XldLoader.Serdes(XldCategory.Automap,2, s, SerdesAutomap, automapIds);
            XldLoader.Serdes(XldCategory.Automap,3, s, SerdesAutomap, automapIds);

            var chestIds = save.Chests.Keys.Select(x => (int) x).ToList();
            chestIds.Add(199);
            chestIds.Add(599);
            XldLoader.Serdes(XldCategory.Chest,0, s, SerdesChest, chestIds);
            XldLoader.Serdes(XldCategory.Chest,1, s, SerdesChest, chestIds);
            XldLoader.Serdes(XldCategory.Chest,2, s, SerdesChest, chestIds);
            XldLoader.Serdes(XldCategory.Chest,5, s, SerdesChest, chestIds);

            var merchantIds = save.Merchants.Keys.Select(x => (int) x).ToList();
            merchantIds.Add(199);
            merchantIds.Add(299);
            XldLoader.Serdes(XldCategory.Merchant,0, s, SerdesMerchant, merchantIds);
            XldLoader.Serdes(XldCategory.Merchant,1, s, SerdesMerchant, merchantIds);
            XldLoader.Serdes(XldCategory.Merchant,2, s, SerdesMerchant, merchantIds);

            var npcIds = save.NpcStats.Keys.Select(x => (int) x).ToList();
            npcIds.Add(299);
            XldLoader.Serdes(XldCategory.NpcCharacter,0, s, SerdesNpcCharacter, npcIds);
            XldLoader.Serdes(XldCategory.NpcCharacter,1, s, SerdesNpcCharacter, npcIds);
            XldLoader.Serdes(XldCategory.NpcCharacter,2, s, SerdesNpcCharacter, npcIds);

            s.RepeatU8("Padding", 0, 4);

            return save;
        }
    }
}

