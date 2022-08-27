using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Containers;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Save;

public class SavedGame
{
    public const int MaxPartySize = 6;
    public const int CombatSlotRows = 2;
    public const int CombatSlotColumns = 6;
    const int MapCount = 512; // TODO
    const int SwitchCount = 1024; // 0x80 bytes
    const int ChestCount = 999; // 7D bytes
    const int DoorCount = 999; // 7D bytes
    const int NpcCountPerMap = 96; // total 49152 = 0x1800 bytes
    const int ChainCountPerMap = 250; // total 128000 = 0x3e80 bytes
    const int AutomapMarkerCount = 256; // = 0x20 bytes
    const int Unk5Count = 1500; // Words? = 0xBC bytes
    const int Unk6Count = 1500; // Words? = 0xBC bytes
    const int Unk8Count = 1500; // ? = 0xBC bytes

    public static readonly DateTime Epoch = new(2200, 1, 1, 0, 0, 0);

    public string Name { get; set; }
    public uint Version { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public MapId MapId { get; set; }
    public MapId MapIdForNpcs { get; set; }
    public ushort PartyX { get; set; }
    public ushort PartyY { get; set; }
    public Direction PartyDirection { get; set; }

    public IDictionary<SheetId, CharacterSheet> Sheets { get; } = new Dictionary<SheetId, CharacterSheet>();
    public IDictionary<AssetId, Inventory> Inventories { get; } = new Dictionary<AssetId, Inventory>(); // TODO: Change to InventoryId?
    public IDictionary<AutomapId, byte[]> Automaps { get; } = new Dictionary<AutomapId, byte[]>();

    readonly FlagSet _switches  = new(SwitchCount);
    readonly FlagSet _unlockedChests  = new(ChestCount);
    readonly FlagSet _unlockedDoors  = new(DoorCount);
    readonly FlagSet _removedNpcs  = new(MapCount, NpcCountPerMap);
    readonly FlagSet _disabledChains  = new(MapCount, ChainCountPerMap);
    readonly FlagSet _automapMarkersFound = new(AutomapMarkerCount);
    readonly TickerSet _tickers = new();

    public IDictionary<TickerId, byte> Tickers => _tickers;
    public bool GetSwitch(SwitchId flag) => _switches.GetFlag(flag.Id);
    public void SetSwitch(SwitchId flag, bool value) => _switches.SetFlag(flag.Id, value);
    public bool IsNpcDisabled(MapId mapId, int npcNumber)
    {
        if (mapId.IsNone)
            mapId = MapId;

        // TODO: Check for possible off-by-one
        return npcNumber is < 0 or >= NpcCountPerMap
               || mapId.Id is < 0 or >= MapCount
               || _removedNpcs.GetFlag(mapId, npcNumber);
    }

    public void SetNpcDisabled(MapId mapId, int npcNumber, bool isDisabled)
    {
        if (mapId.IsNone)
            mapId = MapId;

        if (npcNumber is < 0 or >= NpcCountPerMap)
            return;

        _removedNpcs.SetFlag(mapId, npcNumber, isDisabled);
    }

    public bool IsChainDisabled(MapId mapId, int chainNumber)
    {
        if (mapId.IsNone)
            mapId = MapId;

        // TODO: Check for possible off-by-one
        return chainNumber is < 0 or >= ChainCountPerMap
               || mapId.Id is < 0 or >= MapCount
               || _disabledChains.GetFlag(mapId, chainNumber);
    }

    public void SetChainDisabled(MapId mapId, int chainNumber, bool isDisabled)
    {
        if (mapId.IsNone)
            mapId = MapId;

        if (chainNumber is < 0 or >= ChainCountPerMap)
            return;

        if (mapId.Id is < 0 or >= MapCount)
            return;

        _disabledChains.SetFlag(mapId, chainNumber, isDisabled);
    }

    public bool IsChestOpen(ChestId id) => id.Id is < 0 or >= ChestCount || _unlockedChests.GetFlag(id.Id);
    public bool IsDoorOpen(DoorId id) => id.Id is < 0 or >= DoorCount || _unlockedDoors.GetFlag(id.Id);
    public void SetChestOpen(ChestId id, bool value)
    {
        if (id.Id is < 0 or >= DoorCount) return;
        _unlockedChests.SetFlag(id.Id, value);
    }
    public void SetDoorOpen(DoorId id, bool value)
    {
        if (id.Id is < 0 or >= DoorCount) return;
        _unlockedDoors.SetFlag(id.Id, value);
    }

    public ushort Unk0 { get; set; }
    public uint MagicNumber { get; set; }
    public uint Unk9 { get; set; }
    public ushort[] ActiveSpells { get; set; }
    public byte[] UnkB1 { get; set; }
    public int Unk1A2 { get; set; }
    public ActiveItems ActiveItems { get; set; }
    public ushort HoursSinceResting { get; set; }
    public byte[] CombatPositions { get; private set; } = new byte[MaxPartySize];
    public MiscState Misc { get; private set; } = new();
    public byte[] Unknown5B8c { get; set; }
    public NpcState[] Npcs { get; } = new NpcState[NpcCountPerMap];
    public byte[] Unknown8bb8 { get; set; } 
    public MapChangeCollection PermanentMapChanges { get; private set; } = new();
    public MapChangeCollection TemporaryMapChanges { get; private set; } = new();
    public IList<VisitedEvent> VisitedEvents { get; private set; } = new List<VisitedEvent>();
    public IList<PartyMemberId> ActiveMembers { get; private set; } = new PartyMemberId[MaxPartySize];

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

        save.MagicNumber = s.UInt32(nameof(MagicNumber), save.MagicNumber);
        ApiUtil.Assert(save.MagicNumber == 0x25051971, $"Magic number was expected to be 0x25051971 but it was 0x{save.MagicNumber:x}"); // Must be someone's birthday
        save.Version = s.UInt32(nameof(Version), save.Version);
        ApiUtil.Assert(save.Version == 138, $"Expected save version to be 138, but it was {save.Version}");

        // ------------------------------
        // ---- START OF GAME HEADER ----
        // ------------------------------
        var headerOffset = s.Offset;

        // Comments with hex values after this point are relative to headerOffset
        // (i.e. a watch on "s.Offset - headerOffset" should match the comments while stepping through)
        save.Unk9 = s.UInt32(nameof(Unk9), save.Unk9, 4); // 0
        ushort days = s.UInt16("Days", (ushort)save.ElapsedTime.TotalDays);  // 4
        ushort hours = s.UInt16("Hours", (ushort)save.ElapsedTime.Hours);    // 6
        ushort minutes = s.UInt16("Minutes", (ushort)save.ElapsedTime.Minutes); // 8
        save.ElapsedTime = new TimeSpan(days, hours, minutes, save.ElapsedTime.Seconds, save.ElapsedTime.Milliseconds);
        save.MapId = MapId.SerdesU16(nameof(MapId), save.MapId, mapping, s);      // A
        save.MapIdForNpcs = save.MapId;
        save.PartyX = s.UInt16(nameof(PartyX), save.PartyX);   // C
        save.PartyY = s.UInt16(nameof(PartyY), save.PartyY);   // E
        save.PartyDirection = s.EnumU8(nameof(PartyDirection), save.PartyDirection); // 10

        save.ActiveSpells = (ushort[])s.List( // [0] = Light spell % remaining?, [1] = Max Light%?
            nameof(ActiveSpells),
            save.ActiveSpells,
            0x50,
            (_, x,s2) => s2.UInt16(null, x),
            n => new ushort[n]); // 11

        save.UnkB1 = s.Bytes(nameof(UnkB1), save.UnkB1, 0xE5); // B1

        save.ActiveMembers = s.List( // 196
            nameof(ActiveMembers),
            save.ActiveMembers,
            MaxPartySize,
            (i, x, s2) =>
            {
                var value = PartyMemberId.SerdesU8(null, save.ActiveMembers[i], mapping, s);
                s2.Pad(1);
                return value;
            });

        save.Unk1A2 = s.Int32(nameof(Unk1A2), save.Unk1A2); // 1A2
        save.ActiveItems = s.EnumU32(nameof(ActiveItems), save.ActiveItems); // 1A6
        save.HoursSinceResting = s.UInt16(nameof(HoursSinceResting), save.HoursSinceResting); // 1AA
        save.CombatPositions = s.Bytes(nameof(CombatPositions), save.CombatPositions, MaxPartySize); // 1AC

        save.Misc = s.Object(nameof(Misc), save.Misc, MiscState.Serdes); // 1B2
        save._switches.Serdes("Switches", s); // 272

        // save._unk5Flags.Serdes("Unk5Flags", s);
        // save._unk6Flags.Serdes("Unk6Flags", s);
        // save._unk8Flags.Serdes("Unk8Flags", s);

        // TODO: KnownWord flag dictionaries. Known 3D automap info markers? Battle positions?
        save._disabledChains.Serdes("DisabledChains", s); // 2f2
        save._removedNpcs.Serdes("RemovedNpcs", s); // 2f2 + 3e80 = 4172
        save._automapMarkersFound.Serdes("AutomapMarkers", s); // 5972
        save._unlockedChests.Serdes("UnlockedChests", s); // 5992
        save._unlockedDoors.Serdes("UnlockedDoors", s); // 5A0F
        s.Object(nameof(Tickers), save._tickers, TickerSet.Serdes); // 5A8C

        // ----------------------------
        // ---- END OF GAME HEADER ----
        // ----------------------------

        ApiUtil.Assert(s.Offset - headerOffset == 0x5b8c, $"Expected header to be 0x5b8c bytes, but it was {s.Offset - headerOffset:x}");
        save.Unknown5B8c = s.Bytes(nameof(Unknown5B8c), save.Unknown5B8c, 0x2C);
        var mapType = MapType.TwoD;
        s.List(nameof(save.Npcs), save.Npcs, (mapType, mapping), NpcCountPerMap, NpcState.Serdes); // 5bb8

        save.Unknown8bb8 = s.Bytes( nameof(Unknown8bb8), save.Unknown8bb8, 0x8c0); // 8bb8

        uint permChangesSize = s.UInt32("PermanentMapChanges_Size", (uint)(save.PermanentMapChanges.Count * MapChange.SizeOnDisk + 2)); // 9478
        ushort permChangesCount = s.UInt16("PermanentMapChanges_Count", (ushort)save.PermanentMapChanges.Count); // 947c
        int expectedSize = permChangesCount * MapChange.SizeOnDisk + 2;
        if (permChangesSize != expectedSize)
        {
            ApiUtil.Assert($"Expected perm changes size to be count ({permChangesCount}) * {MapChange.SizeOnDisk} + 2 == {expectedSize}, but it was {permChangesSize}");
            // When the size and count disagree the size seems to be more reliable
            permChangesCount = (ushort)((permChangesSize - 2) / MapChange.SizeOnDisk);
        }

        save.PermanentMapChanges = (MapChangeCollection)s.List( // 947e
            nameof(PermanentMapChanges),
            save.PermanentMapChanges,
            mapping,
            permChangesCount,
            MapChange.Serdes,
            _ => new MapChangeCollection());

        uint tempChangesSize = s.UInt32("TemporaryMapChanges_Size", (uint)(save.TemporaryMapChanges.Count * MapChange.SizeOnDisk + 2));
        ushort tempChangesCount = s.UInt16("TemporaryMapChanges_Count", (ushort)save.TemporaryMapChanges.Count);

        expectedSize = tempChangesCount * MapChange.SizeOnDisk + 2;
        if (tempChangesSize != expectedSize)
        {
            ApiUtil.Assert($"Expected temp changes size to be count ({tempChangesCount}) * {MapChange.SizeOnDisk} + 2 == {expectedSize}, but it was {tempChangesSize}");
            tempChangesCount = (ushort)((tempChangesSize - 2) / MapChange.SizeOnDisk);
        }

        save.TemporaryMapChanges = (MapChangeCollection)s.List(
            nameof(TemporaryMapChanges),
            save.TemporaryMapChanges,
            mapping,
            tempChangesCount,
            MapChange.Serdes,
            _ => new MapChangeCollection());

        uint visitedEventsSize = s.UInt32("VisitedEvents_Size", (uint)(save.VisitedEvents.Count * VisitedEvent.SizeOnDisk + 2));
        ushort visitedEventsCount = s.UInt16("VisitedEvents_Count", (ushort)save.VisitedEvents.Count);

        expectedSize = visitedEventsCount * VisitedEvent.SizeOnDisk + 2;
        if (visitedEventsSize != expectedSize)
        {
            ApiUtil.Assert($"Expected visited events size to be count ({visitedEventsCount}) * {VisitedEvent.SizeOnDisk} + 2 == {expectedSize}, but it was {visitedEventsSize}");
            visitedEventsCount = (ushort)((visitedEventsSize - 2) / VisitedEvent.SizeOnDisk);
        }

        save.VisitedEvents = s.List(nameof(VisitedEvents), save.VisitedEvents, mapping, visitedEventsCount, VisitedEvent.Serdes);

        var partyIds = save.Sheets.Keys.Where(x => x.Type == AssetType.PartySheet).Select(x => x.Id).ToList();
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

        s.Pad("Padding", 4);

        // TODO: Save additional sheets & inventories from mods.

        return save;
    }

    static void SerdesPartyCharacter(int i, int size, (SavedGame save, AssetMapping mapping, ISpellManager spellManager) context, ISerializer serializer)
    {
        if (i > 0xff)
            return;

        var id = SheetId.FromDisk(AssetType.PartySheet, i, context.mapping);
        CharacterSheet existing = null;
        if (size > 0 || context.save.Sheets.TryGetValue(id, out existing))
            context.save.Sheets[id] = CharacterSheet.Serdes(id, existing, context.mapping, serializer, context.spellManager);
    }

    static void SerdesNpcCharacter(int i, int size, (SavedGame save, AssetMapping mapping, ISpellManager spellManager) context, ISerializer serializer)
    {
        if (i > 0xff)
            return;

        var id = SheetId.FromDisk(AssetType.NpcSheet, i, context.mapping);
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

    public bool IsEventUsed(AssetId eventSetId, ActionEvent action)
    {
        foreach (var e in VisitedEvents)
            if (e.EventSetId == eventSetId && e.Type == action.ActionType && e.Argument == action.Argument)
                return true;

        return false;
    }

    public void UseEvent(AssetId eventSetId, ActionEvent action)
    {
        if (IsEventUsed(eventSetId, action))
            return;

        VisitedEvents.Add(new VisitedEvent(eventSetId, action.ActionType, action.Argument));
    }
}
