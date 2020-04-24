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

        public string Name { get; set; }
        public ushort Version { get; set; }

        public ushort Days { get; set; }
        public ushort Hours { get; set; }
        public ushort Minutes { get; set; }

        public MapDataId MapId { get; set; }
        public ushort PartyX { get; set; }
        public ushort PartyY { get; set; }
        public Direction PartyDirection { get; set; }

        public IDictionary<PartyCharacterId, CharacterSheet> PartyMembers { get; } = new Dictionary<PartyCharacterId, CharacterSheet>();
        public IDictionary<NpcCharacterId, CharacterSheet> Npcs { get; } = new Dictionary<NpcCharacterId, CharacterSheet>();
        public IDictionary<AutoMapId, byte[]> Automaps { get; } = new Dictionary<AutoMapId, byte[]>();
        public IDictionary<ChestId, Chest> Chests { get; } = new Dictionary<ChestId, Chest>();
        public IDictionary<MerchantId, Chest> Merchants { get; } = new Dictionary<MerchantId, Chest>();
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
        public byte[] Unknown5B71 { get; set; }
        public MapChangeList PermanentMapChanges { get; set; }
        public MapChangeList TemporaryMapChanges { get; set; }
        public MysteryChunk6 Mystery6 { get; set; }
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

            save.Days    = s.UInt16(nameof(Days), save.Days);       // 8
            save.Hours   = s.UInt16(nameof(Hours), save.Hours);     // A
            save.Minutes = s.UInt16(nameof(Minutes), save.Minutes); // C
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
            save._tickers.Serdes(s); // 5AF4

            // Most likely NPC info, 128 bytes per record.
            save.Unknown5B71 = s.ByteArrayHex(
                nameof(Unknown5B71),
                save.Unknown5B71,
                (int)(0x947C + versionOffset - s.Offset)); // 5B71

            save.PermanentMapChanges = s.Meta(
                nameof(PermanentMapChanges),
                save.PermanentMapChanges,
                MapChangeList.Serdes);

            save.TemporaryMapChanges = s.Meta(
                nameof(TemporaryMapChanges),
                save.TemporaryMapChanges,
                MapChangeList.Serdes);

            save.Mystery6 = s.Meta(nameof(Mystery6), save.Mystery6, MysteryChunk6.Serdes);

            var charLoader = new CharacterSheetLoader();
            var chestLoader = new ChestLoader();
            void SerdesPartyCharacter(int i, int size, ISerializer serializer)
            {
                if (i > 0xff)
                    return;

                var key = (PartyCharacterId)i;
                CharacterSheet existing = null;
                if (size > 0 || save.PartyMembers.TryGetValue(key, out existing))
                    save.PartyMembers[key] = charLoader.Serdes(existing, serializer, key.ToString(), null);
            }

            void SerdesNpcCharacter(int i, int size, ISerializer serializer)
            {
                if (i > 0xff)
                    return;

                var key = (NpcCharacterId)i;
                CharacterSheet existing = null;
                if (serializer.Mode == SerializerMode.Reading || save.Npcs.TryGetValue(key, out existing))
                    save.Npcs[key] = charLoader.Serdes(existing, serializer, key.ToString(), null);
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
                Chest existing = null;
                var chestConfig = new BasicAssetInfo
                {
                    Id = i,
                    Parent = new BasicXldInfo
                    {
                        Format = FileFormat.Inventory
                    }
                };

                if (serializer.Mode == SerializerMode.Reading || save.Chests.TryGetValue(key, out existing))
                    save.Chests[key] = chestLoader.Serdes(existing, serializer, key.ToString(), chestConfig);
            }

            void SerdesMerchant(int i, int size, ISerializer serializer)
            {
                if (i > 0xff)
                    return;

                var key = (MerchantId)i;
                Chest existing = null;
                var merchantConfig = new BasicAssetInfo
                {
                    Id = i,
                    Parent = new BasicXldInfo
                    {
                        Format = FileFormat.MerchantInventory
                    }
                };

                if (serializer.Mode == SerializerMode.Reading || save.Merchants.TryGetValue(key, out existing))
                    save.Merchants[key] = chestLoader.Serdes(existing, serializer, key.ToString(), merchantConfig);
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

            var npcIds = save.Npcs.Keys.Select(x => (int) x).ToList();
            npcIds.Add(299);
            XldLoader.Serdes(XldCategory.NpcCharacter,0, s, SerdesNpcCharacter, npcIds);
            XldLoader.Serdes(XldCategory.NpcCharacter,1, s, SerdesNpcCharacter, npcIds);
            XldLoader.Serdes(XldCategory.NpcCharacter,2, s, SerdesNpcCharacter, npcIds);

            s.RepeatU8("Padding", 0, 4);

            return save;
        }
    }
}

