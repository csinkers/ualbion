using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Containers;

namespace UAlbion.Formats.Assets.Save;

public class SavedGame
{
    public const int MaxPartySize = 6;
    public const int MaxNpcCount = 96;
    public static readonly DateTime Epoch = new(2200, 1, 1, 0, 0, 0);

    public string Name { get; set; }
    public ushort Version { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public MapId MapId { get; set; }
    public ushort PartyX { get; set; }
    public ushort PartyY { get; set; }
    public Direction PartyDirection { get; set; }

    public IDictionary<CharacterId, CharacterSheet> Sheets { get; } = new Dictionary<CharacterId, CharacterSheet>();
    public IDictionary<AssetId, Inventory> Inventories { get; } = new Dictionary<AssetId, Inventory>(); // TODO: Change to InventoryId?
    public IDictionary<AutomapId, byte[]> Automaps { get; } = new Dictionary<AutomapId, byte[]>();

    readonly TickerDictionary _tickers  = new();
    readonly FlagDictionary _switches  = new();
    public IDictionary<TickerId, byte> Tickers => _tickers;
    public bool GetFlag(SwitchId flag) => _switches.GetFlag(flag);
    public void SetFlag(SwitchId flag, bool value) => _switches.SetFlag(flag, value);

    public ushort Unk0 { get; set; }
    public uint Unk1 { get; set; }
    public byte[] Unk9 { get; set; }
    public byte[] Unknown15 { get; set; }
    public MiscState Misc { get; private set; } = new();
    public byte[] Unknown2C1 { get; set; }
    public byte[] Unknown5B9F { get; set; }
    public NpcState[] Npcs { get; } = new NpcState[MaxNpcCount];
    public byte[] Unknown5B71 { get; set; } 
    public MapChangeCollection PermanentMapChanges { get; private set; } = new();
    public MapChangeCollection TemporaryMapChanges { get; private set; } = new();
    public IList<VisitedEvent> VisitedEvents { get; private set; } = new List<VisitedEvent>();
    public IList<PartyMemberId> ActiveMembers { get; private set; } = new PartyMemberId[MaxPartySize];
    public ISet<(AssetId, ushort)> DisabledChains { get; } = new HashSet<(AssetId, ushort)>();

    public static string GetName(BinaryReader br)
    {
        if (br == null) throw new ArgumentNullException(nameof(br));
        using var s = new AlbionReader(br, br.BaseStream.Length);
        ushort nameLength = s.UInt16("NameLength", 0);
        if (nameLength > 1024)
            return "Invalid";

        s.UInt16(nameof(Unk0), 0);
        return s.FixedLengthString(nameof(Name), null, nameLength);
    }

    public static SavedGame Serdes(SavedGame save, AssetMapping mapping, ISerializer s, ISpellManager spellManager)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        save ??= new SavedGame();

        ushort nameLength = s.UInt16("NameLength", (ushort)(save.Name?.Length ?? 0));
        save.Unk0 = s.UInt16(nameof(Unk0), save.Unk0);
        save.Name = s.FixedLengthString(nameof(Name), save.Name, nameLength);

        save.Unk1 = s.UInt32(nameof(Unk1), save.Unk1);
        var versionOffset = s.Offset;
        save.Version = s.UInt16(nameof(Version), save.Version); // 0
        ApiUtil.Assert(save.Version == 138); // TODO: Throw error for other versions?
        save.Unk9 = s.Bytes(nameof(Unk9), save.Unk9, 6); // 2

        ushort days = s.UInt16("Days", (ushort)save.ElapsedTime.TotalDays);  // 8
        ushort hours = s.UInt16("Hours", (ushort)save.ElapsedTime.Hours);    // A
        ushort minutes = s.UInt16("Minutes", (ushort)save.ElapsedTime.Minutes); // C
        save.ElapsedTime = new TimeSpan(days, hours, minutes, save.ElapsedTime.Seconds, save.ElapsedTime.Milliseconds);
        save.MapId = MapId.SerdesU16(nameof(MapId), save.MapId, mapping, s);      // E
        save.PartyX = s.UInt16(nameof(PartyX), save.PartyX);   // 10
        save.PartyY = s.UInt16(nameof(PartyY), save.PartyY);   // 12
        save.PartyDirection = s.EnumU8(nameof(PartyDirection), save.PartyDirection); // 14
        save.Unknown15 = s.Bytes(nameof(Unknown15), save.Unknown15, 0x185); // 15 TODO: Active spell info?

        save.ActiveMembers = s.List(
            nameof(ActiveMembers),
            save.ActiveMembers,
            MaxPartySize,
            (i, x, s2) =>
            {
                var value = PartyMemberId.SerdesU8(null, save.ActiveMembers[i], mapping, s);
                s2.Pad(1);
                return value;
            });

        save.Misc = s.Object(nameof(Misc), save.Misc, MiscState.Serdes); // 1A6
        save._switches.SetPacked(0,
            s.Bytes("Switches",
                save._switches.GetPacked(0, FlagDictionary.OriginalSaveGameMax, mapping),
                FlagDictionary.PackedSize(0, FlagDictionary.OriginalSaveGameMax)), mapping); // 276

        // TODO: Chain, Door, Chest, Npc, KnownWord flag dictionaries. Known 3D automap info markers? Battle positions?
        save.Unknown2C1 = s.Bytes(nameof(Unknown2C1), save.Unknown2C1, 0x5833); // 0x2C1
        s.Object(nameof(Tickers), save._tickers, TickerDictionary.Serdes); // 5AF4

        save.Unknown5B9F = s.Bytes(nameof(Unknown5B9F), save.Unknown5B9F, 0x2C);
        var mapType = MapType.TwoD;
        s.List(nameof(save.Npcs), save.Npcs, (mapType, mapping), MaxNpcCount, NpcState.Serdes);

        save.Unknown5B71 = s.Bytes(
            nameof(Unknown5B71),
            save.Unknown5B71,
            0x8c0); // (int)(0x947C + versionOffset - s.Offset)); // 5B9F

        uint permChangesSize = s.UInt32("PermanentMapChanges_Size", (uint)(save.PermanentMapChanges.Count * MapChange.SizeOnDisk + 2));
        ushort permChangesCount = s.UInt16("PermanentMapChanges_Count", (ushort)save.PermanentMapChanges.Count);
        ApiUtil.Assert(permChangesSize == permChangesCount * MapChange.SizeOnDisk + 2);
        save.PermanentMapChanges = (MapChangeCollection)s.List(
            nameof(PermanentMapChanges),
            save.PermanentMapChanges,
            mapping,
            permChangesCount,
            MapChange.Serdes,
            _ => new MapChangeCollection());

        uint tempChangesSize = s.UInt32("TemporaryMapChanges_Size", (uint)(save.TemporaryMapChanges.Count * MapChange.SizeOnDisk + 2));
        ushort tempChangesCount = s.UInt16("TemporaryMapChanges_Count", (ushort)save.TemporaryMapChanges.Count);
        ApiUtil.Assert(tempChangesSize == tempChangesCount * MapChange.SizeOnDisk + 2);

        save.TemporaryMapChanges = (MapChangeCollection)s.List(
            nameof(TemporaryMapChanges),
            save.TemporaryMapChanges,
            mapping,
            tempChangesCount,
            MapChange.Serdes,
            _ => new MapChangeCollection());

        uint visitedEventsSize = s.UInt32("VisitedEvents_Size", (uint)(save.VisitedEvents.Count * VisitedEvent.SizeOnDisk + 2));
        ushort visitedEventsCount = s.UInt16("VisitedEvents_Count", (ushort)save.VisitedEvents.Count);
        ApiUtil.Assert(visitedEventsSize == visitedEventsCount * VisitedEvent.SizeOnDisk + 2);
        save.VisitedEvents = s.List(nameof(VisitedEvents), save.VisitedEvents, mapping, visitedEventsCount, VisitedEvent.Serdes);

        var partyIds = save.Sheets.Keys.Where(x => x.Type == AssetType.Party).Select(x => x.Id).ToList();
        partyIds.Add(199); // Force extra XLD length fields to be written for empty objects to preserve compat with original game.
        partyIds.Add(299);

        // s.Object($"XldPartyCharacter.0");
        var context2 = (save, mapping);
        var context3 = (save, mapping, spellManager);

        XldContainer.Serdes(XldCategory.PartyCharacter, 1, 99, context3, s, SerdesPartyCharacter, partyIds);
        XldContainer.Serdes(XldCategory.PartyCharacter, 100, 199, context3, s, SerdesPartyCharacter, partyIds);
        XldContainer.Serdes(XldCategory.PartyCharacter, 200, 299, context3, s, SerdesPartyCharacter, partyIds);

        var automapIds = save.Automaps.Keys.Select(x => x.Id).ToList(); // TODO: Allow extension somehow
        automapIds.Add(199);
        automapIds.Add(399);
        XldContainer.Serdes(XldCategory.Automap, 100, 199, context2, s, SerdesAutomap, automapIds);
        XldContainer.Serdes(XldCategory.Automap, 200, 299, context2, s, SerdesAutomap, automapIds);
        XldContainer.Serdes(XldCategory.Automap, 300, 399, context2, s, SerdesAutomap, automapIds);

        var chestIds = save.Inventories.Keys.Where(x => x.Type == AssetType.Chest).Select(x => x.Id).ToList(); // TODO: Allow extension somehow
        chestIds.Add(199);
        chestIds.Add(599);
        XldContainer.Serdes(XldCategory.Chest, 1, 99, context2, s, SerdesChest, chestIds);
        XldContainer.Serdes(XldCategory.Chest, 100, 199, context2, s, SerdesChest, chestIds);
        XldContainer.Serdes(XldCategory.Chest, 200, 299, context2, s, SerdesChest, chestIds);
        XldContainer.Serdes(XldCategory.Chest, 500, 599, context2, s, SerdesChest, chestIds);

        var merchantIds = save.Inventories.Keys.Where(x => x.Type == AssetType.Merchant).Select(x => x.Id).ToList(); // TODO: Allow extension somehow
        merchantIds.Add(199);
        merchantIds.Add(299);
        XldContainer.Serdes(XldCategory.Merchant, 1, 99, context2, s, SerdesMerchant, merchantIds);
        XldContainer.Serdes(XldCategory.Merchant, 100, 199, context2, s, SerdesMerchant, merchantIds);
        XldContainer.Serdes(XldCategory.Merchant, 200, 299, context2, s, SerdesMerchant, merchantIds);

        var npcIds = save.Sheets.Keys.Select(x => x.Id).ToList(); // TODO: Allow extension somehow
        npcIds.Add(299);
        XldContainer.Serdes(XldCategory.NpcCharacter, 1, 99, context3, s, SerdesNpcCharacter, npcIds);
        XldContainer.Serdes(XldCategory.NpcCharacter, 100, 199, context3, s, SerdesNpcCharacter, npcIds);
        XldContainer.Serdes(XldCategory.NpcCharacter, 200, 299, context3, s, SerdesNpcCharacter, npcIds);

        s.RepeatU8("Padding", 0, 4);

        // TODO: Save additional sheets & inventories from mods.

        return save;
    }

    static void SerdesPartyCharacter(int i, int size, (SavedGame save, AssetMapping mapping, ISpellManager spellManager) context, ISerializer serializer)
    {
        if (i > 0xff)
            return;

        var id = CharacterId.FromDisk(AssetType.Party, i, context.mapping);
        CharacterSheet existing = null;
        if (size > 0 || context.save.Sheets.TryGetValue(id, out existing))
            context.save.Sheets[id] = CharacterSheet.Serdes(id, existing, context.mapping, serializer, context.spellManager);
    }

    static void SerdesNpcCharacter(int i, int size, (SavedGame save, AssetMapping mapping, ISpellManager spellManager) context, ISerializer serializer)
    {
        if (i > 0xff)
            return;

        var id = CharacterId.FromDisk(AssetType.Npc, i, context.mapping);
        CharacterSheet existing = null;
        if (serializer.IsReading() || context.save.Sheets.TryGetValue(id, out existing))
            context.save.Sheets[id] = CharacterSheet.Serdes(id, existing, context.mapping, serializer, context.spellManager);
    }

    static void SerdesAutomap(int i, int size, (SavedGame, AssetMapping) context, ISerializer serializer)
    {
        var save = context.Item1;
        var mapping = context.Item2;
        var key = AutomapId.FromDisk(i, mapping);
        if (save.Automaps.TryGetValue(key, out _))
            serializer.Bytes(null, save.Automaps[key], save.Automaps[key].Length);
        else if (serializer.IsReading())
            save.Automaps[key] = serializer.Bytes(null, null, size);
    }

    static void SerdesChest(int i, int size, (SavedGame, AssetMapping) context, ISerializer serializer)
    {
        var save = context.Item1;
        var mapping = context.Item2;
        var key = ChestId.FromDisk(i, mapping);
        Inventory existing = null;

        if (serializer.IsReading() || save.Inventories.TryGetValue(key, out existing))
            save.Inventories[key] = Inventory.SerdesChest(i, existing, mapping, serializer);
    }

    static void SerdesMerchant(int i, int size, (SavedGame, AssetMapping) context, ISerializer serializer)
    {
        if (i > 0xff)
            return;

        var save = context.Item1;
        var mapping = context.Item2;
        var key = MerchantId.FromDisk(i, mapping);
        Inventory existing = null;

        if (serializer.IsReading() || save.Inventories.TryGetValue(key, out existing))
            save.Inventories[key] = Inventory.SerdesMerchant(i, existing, mapping, serializer);
    }
}