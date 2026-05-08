using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Containers;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Save;

public class SavedGame
{
    public const int MaxPartySize = 6;
    public const int CombatRows = 5;
    public const int CombatRowsForParty = 2;
    public const int CombatRowsForMobs = CombatRows - CombatRowsForParty;
    public const int CombatColumns = 6;
    const int MapCount = 512;
    const int SwitchCount = 1024;
    const int ChestCount = 999;
    const int DoorCount = 999;
    const int NpcCountPerMap = 96;
    const int ChainCountPerMap = 250;
    const int AutomapMarkerCount = 256;

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
    public IDictionary<AssetId, Inventory> Inventories { get; } = new Dictionary<AssetId, Inventory>();
    public IDictionary<AutomapId, byte[]> Automaps { get; } = new Dictionary<AutomapId, byte[]>();

    readonly FlagSet _switches = new(SwitchCount);
    readonly FlagSet _unlockedChests = new(ChestCount);
    readonly FlagSet _unlockedDoors = new(DoorCount);
    readonly FlagSet _removedNpcs = new(MapCount, NpcCountPerMap);
    readonly FlagSet _disabledChains = new(MapCount, ChainCountPerMap);
    readonly FlagSet _automapMarkersFound = new(AutomapMarkerCount);
    readonly TickerSet _tickers = [];

    public IDictionary<TickerId, byte> Tickers => _tickers;
    public bool GetSwitch(SwitchId flag) => _switches.GetFlag(flag.Id);
    public void SetSwitch(SwitchId flag, bool value) => _switches.SetFlag(flag.Id, value);
    public bool IsNpcDisabled(MapId mapId, int npcNumber)
    {
        if (mapId.IsNone)
            mapId = MapId;

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
    public ushort[] ActiveSpells { get; set; } = new ushort[0x50];
    public byte[] UnkB1 { get; set; } = new byte[0xE5];
    public int Unk1A2 { get; set; }
    public ActiveItems ActiveItems { get; set; }
    public ushort HoursSinceResting { get; set; }
    public byte[] CombatPositions { get; private set; } = new byte[MaxPartySize];
    public MiscState Misc { get; private set; } = new();
    public byte[] Unknown5B8C { get; set; } = new byte[0x2C];
    public NpcState[] Npcs { get; } = new NpcState[NpcCountPerMap];
    public byte[] Unknown8Bb8 { get; set; } = new byte[0x8c0];
    public MapChangeCollection PermanentMapChanges { get; private set; } = [];
    public MapChangeCollection TemporaryMapChanges { get; private set; } = [];
    HashSet<VisitedEvent> _visitedSet = [];
    List<VisitedEvent> _visitedEvents = [];
    public IReadOnlyList<VisitedEvent> VisitedEvents => _visitedEvents;
    public IList<PartyMemberId> ActiveMembers { get; private set; } = new PartyMemberId[MaxPartySize];

    public static string GetName(ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        ushort nameLength = s.UInt16("NameLength", 0);
        if (nameLength > 1024)
            return "Invalid";

        s.UInt16(nameof(Unk0), 0);
        return s.AlbionString(nameof(Name), null, nameLength);
    }

    public static SavedGame Serdes(SavedGame save, AssetMapping mapping, ISerdes s, ISpellManager spellManager)
    {
        ArgumentNullException.ThrowIfNull(s);
        save ??= new SavedGame();

        ushort nameLength = s.UInt16("NameLength", (ushort)(save.Name?.Length ?? 0));
        save.Unk0 = s.UInt16(nameof(Unk0), save.Unk0);
        save.Name = s.AlbionString(nameof(Name), save.Name, nameLength);

        save.MagicNumber = s.UInt32(nameof(MagicNumber), save.MagicNumber);
        ApiUtil.Assert(save.MagicNumber == 0x25051971, $"Magic number was expected to be 0x25051971 but it was 0x{save.MagicNumber:x}");
        save.Version = s.UInt32(nameof(Version), save.Version);
        ApiUtil.Assert(save.Version == 138, $"Expected save version to be 138, but it was {save.Version}");

        var headerOffset = s.Offset;

        save.Unk9 = s.UInt32(nameof(Unk9), save.Unk9);
        ushort days = s.UInt16("Days", (ushort)save.ElapsedTime.TotalDays);
        ushort hours = s.UInt16("Hours", (ushort)save.ElapsedTime.Hours);
        ushort minutes = s.UInt16("Minutes", (ushort)save.ElapsedTime.Minutes);
        save.ElapsedTime = new TimeSpan(days, hours, minutes, save.ElapsedTime.Seconds, save.ElapsedTime.Milliseconds);
        save.MapId = MapId.SerdesU16(nameof(MapId), save.MapId, mapping, s);
        save.MapIdForNpcs = save.MapId;
        save.PartyX = s.UInt16(nameof(PartyX), save.PartyX);
        save.PartyY = s.UInt16(nameof(PartyY), save.PartyY);
        save.PartyDirection = s.EnumU8(nameof(PartyDirection), save.PartyDirection);

        save.ActiveSpells = (ushort[])s.List(
            nameof(ActiveSpells),
            save.ActiveSpells,
            0x50,
            (_, x, s2) => s2.UInt16(null, x),
            n => new ushort[n]);

        save.UnkB1 = s.Bytes(nameof(UnkB1), save.UnkB1, 0xE5);

        save.ActiveMembers = s.List(
            nameof(ActiveMembers),
            save.ActiveMembers,
            MaxPartySize,
            (name, _, s2) =>
            {
                var value = PartyMemberId.SerdesU8(null, save.ActiveMembers[name.N], mapping, s);
                s2.Pad(1);
                return value;
            });

        save.Unk1A2 = s.Int32(nameof(Unk1A2), save.Unk1A2);
        save.ActiveItems = s.EnumU32(nameof(ActiveItems), save.ActiveItems);
        save.HoursSinceResting = s.UInt16(nameof(HoursSinceResting), save.HoursSinceResting);
        save.CombatPositions = s.Bytes(nameof(CombatPositions), save.CombatPositions, MaxPartySize);

        save.Misc = s.Object(nameof(Misc), save.Misc, MiscState.Serdes);
        save._switches.Serdes("Switches", s);

        save._disabledChains.Serdes("DisabledChains", s);
        save._removedNpcs.Serdes("RemovedNpcs", s);
        save._automapMarkersFound.Serdes("AutomapMarkers", s);
        save._unlockedChests.Serdes("UnlockedChests", s);
        save._unlockedDoors.Serdes("UnlockedDoors", s);
        s.Object(nameof(Tickers), save._tickers, TickerSet.Serdes);

        ApiUtil.Assert(s.Offset - headerOffset == 0x5b8c, $"Expected header to be 0x5b8c bytes, but it was {s.Offset - headerOffset:x}");
        save.Unknown5B8C = s.Bytes(nameof(Unknown5B8C), save.Unknown5B8C, 0x2C);
        var mapType = MapType.TwoD;
        s.ListWithContext(nameof(save.Npcs), save.Npcs, (mapType, mapping), NpcCountPerMap, NpcState.Serdes);

        save.Unknown8Bb8 = s.Bytes(nameof(Unknown8Bb8), save.Unknown8Bb8, 0x8c0);

        SerdesPermanentMapChanges(save, mapping, s);
        SerdesTemporaryMapChanges(save, mapping, s);
        SerdesVisitEventIds(save, mapping, s);

        SerdesPartyCharacters(save, mapping, spellManager, s);
        SerdesAutomaps(save, mapping, s);
        SerdesChests(save, mapping, s);
        SerdesMerchants(save, mapping, s);
        SerdesNpcCharacters(save, mapping, spellManager, s);
        s.Pad("Padding", 4);

        return save;
    }

    static void SerdesPartyCharacters(SavedGame save, AssetMapping mapping, ISpellManager spellManager, ISerdes s)
    {
        var partyIds = save.Sheets.Keys.Where(x => x.Type == AssetType.PartySheet).Select(x => x.Id).ToList();
        partyIds.Add(99);
        partyIds.Add(199);
        partyIds.Add(299);

        var context = (save, mapping, spellManager);
        XldContainer.Serdes(XldCategory.PartyCharacter, 1, 99, context, s, SerdesPartyCharacter, partyIds);
        XldContainer.Serdes(XldCategory.PartyCharacter, 100, 199, context, s, SerdesPartyCharacter, partyIds);
        XldContainer.Serdes(XldCategory.PartyCharacter, 200, 299, context, s, SerdesPartyCharacter, partyIds);
    }

    static void SerdesNpcCharacters(SavedGame save, AssetMapping mapping, ISpellManager spellManager, ISerdes s)
    {
        var npcIds = save.Sheets.Keys.Select(x => x.Id).ToList();
        npcIds.Add(99);
        npcIds.Add(199);
        npcIds.Add(299);
        var context = (save, mapping, spellManager);
        XldContainer.Serdes(XldCategory.NpcCharacter, 1, 99, context, s, SerdesNpcCharacter, npcIds);
        XldContainer.Serdes(XldCategory.NpcCharacter, 100, 199, context, s, SerdesNpcCharacter, npcIds);
        XldContainer.Serdes(XldCategory.NpcCharacter, 200, 299, context, s, SerdesNpcCharacter, npcIds);
    }

    static void SerdesAutomaps(SavedGame save, AssetMapping mapping, ISerdes s)
    {
        var automapIds = save.Automaps.Keys.Select(x => x.Id).ToList();
        automapIds.Add(199);
        automapIds.Add(299);
        automapIds.Add(399);

        var context = (save, mapping);
        XldContainer.Serdes(XldCategory.Automap, 100, 199, context, s, SerdesAutomap, automapIds);
        XldContainer.Serdes(XldCategory.Automap, 200, 299, context, s, SerdesAutomap, automapIds);
        XldContainer.Serdes(XldCategory.Automap, 300, 399, context, s, SerdesAutomap, automapIds);
    }

    static void SerdesChests(SavedGame save, AssetMapping mapping, ISerdes s)
    {
        var chestIds = save.Inventories.Keys.Where(x => x.Type == AssetType.Chest).Select(x => x.Id).ToList();
        chestIds.Add(99);
        chestIds.Add(199);
        chestIds.Add(299);
        chestIds.Add(599);

        var context = (save, mapping);
        XldContainer.Serdes(XldCategory.Chest, 1, 99, context, s, SerdesChest, chestIds);
        XldContainer.Serdes(XldCategory.Chest, 100, 199, context, s, SerdesChest, chestIds);
        XldContainer.Serdes(XldCategory.Chest, 200, 299, context, s, SerdesChest, chestIds);
        XldContainer.Serdes(XldCategory.Chest, 500, 599, context, s, SerdesChest, chestIds);
    }

    static void SerdesMerchants(SavedGame save, AssetMapping mapping, ISerdes s)
    {
        var merchantIds = save.Inventories.Keys.Where(x => x.Type == AssetType.Merchant).Select(x => x.Id).ToList();
        merchantIds.Add(99);
        merchantIds.Add(199);
        merchantIds.Add(299);

        var context = (save, mapping);
        XldContainer.Serdes(XldCategory.Merchant, 1, 99, context, s, SerdesMerchant, merchantIds);
        XldContainer.Serdes(XldCategory.Merchant, 100, 199, context, s, SerdesMerchant, merchantIds);
        XldContainer.Serdes(XldCategory.Merchant, 200, 299, context, s, SerdesMerchant, merchantIds);
    }

    static void SerdesPermanentMapChanges(SavedGame save, AssetMapping mapping, ISerdes s)
    {
        uint permChangesSize = s.UInt32("PermanentMapChanges_Size", (uint)(save.PermanentMapChanges.Count * MapChange.SizeOnDisk + 2));
        ushort permChangesCount = s.UInt16("PermanentMapChanges_Count", (ushort)save.PermanentMapChanges.Count);
        int expectedSize = permChangesCount * MapChange.SizeOnDisk + 2;
        if (permChangesSize != expectedSize)
        {
            ApiUtil.Assert($"Expected perm changes size to be count ({permChangesCount}) * {MapChange.SizeOnDisk} + 2 == {expectedSize}, but it was {permChangesSize}");
            permChangesCount = (ushort)((permChangesSize - 2) / MapChange.SizeOnDisk);
        }

        save.PermanentMapChanges = (MapChangeCollection)s.ListWithContext(
            nameof(PermanentMapChanges),
            save.PermanentMapChanges,
            mapping,
            permChangesCount,
            MapChange.Serdes,
            _ => (MapChangeCollection)[]);
    }

    static void SerdesTemporaryMapChanges(SavedGame save, AssetMapping mapping, ISerdes s)
    {
        uint tempChangesSize = s.UInt32("TemporaryMapChanges_Size", (uint)(save.TemporaryMapChanges.Count * MapChange.SizeOnDisk + 2));
        ushort tempChangesCount = s.UInt16("TemporaryMapChanges_Count", (ushort)save.TemporaryMapChanges.Count);

        var expectedSize = tempChangesCount * MapChange.SizeOnDisk + 2;
        if (tempChangesSize != expectedSize)
        {
            ApiUtil.Assert($"Expected temp changes size to be count ({tempChangesCount}) * {MapChange.SizeOnDisk} + 2 == {expectedSize}, but it was {tempChangesSize}");
            tempChangesCount = (ushort)((tempChangesSize - 2) / MapChange.SizeOnDisk);
        }

        save.TemporaryMapChanges = (MapChangeCollection)s.ListWithContext(
            nameof(TemporaryMapChanges),
            save.TemporaryMapChanges,
            mapping,
            tempChangesCount,
            MapChange.Serdes,
            _ => (MapChangeCollection)[]);
    }

    static void SerdesVisitEventIds(SavedGame save, AssetMapping mapping, ISerdes s)
    {
        uint visitedEventsSize = s.UInt32("VisitedEvents_Size", (uint)(save.VisitedEvents.Count * VisitedEvent.SizeOnDisk + 2));
        ushort visitedEventsCount = s.UInt16("VisitedEvents_Count", (ushort)save.VisitedEvents.Count);

        var expectedSize = visitedEventsCount * VisitedEvent.SizeOnDisk + 2;
        if (visitedEventsSize != expectedSize)
        {
            ApiUtil.Assert($"Expected visited events size to be count ({visitedEventsCount}) * {VisitedEvent.SizeOnDisk} + 2 == {expectedSize}, but it was {visitedEventsSize}");
            visitedEventsCount = (ushort)((visitedEventsSize - 2) / VisitedEvent.SizeOnDisk);
        }

        save._visitedEvents =
            (List<VisitedEvent>)s.ListWithContext(
                nameof(VisitedEvents),
                save._visitedEvents,
                mapping,
                visitedEventsCount,
                VisitedEvent.Serdes);
        save._visitedSet = [.. save._visitedEvents];
    }

    static void SerdesPartyCharacter(int i, int size, (SavedGame save, AssetMapping mapping, ISpellManager spellManager) context, ISerdes serdes)
    {
        if (i > 0xff)
            return;

        var id = SheetId.FromDisk(AssetType.PartySheet, i, context.mapping);
        CharacterSheet existing = null;
        if (size > 0 || context.save.Sheets.TryGetValue(id, out existing))
            context.save.Sheets[id] = CharacterSheet.Serdes(id, existing, context.mapping, serdes, context.spellManager);
    }

    static void SerdesNpcCharacter(int i, int size, (SavedGame save, AssetMapping mapping, ISpellManager spellManager) context, ISerdes serdes)
    {
        if (i > 0xff)
            return;

        var id = SheetId.FromDisk(AssetType.NpcSheet, i, context.mapping);
        CharacterSheet existing = null;
        if (serdes.IsReading() || context.save.Sheets.TryGetValue(id, out existing))
            context.save.Sheets[id] = CharacterSheet.Serdes(id, existing, context.mapping, serdes, context.spellManager);
    }

    static void SerdesAutomap(int i, int size, (SavedGame, AssetMapping) context, ISerdes serdes)
    {
        var save = context.Item1;
        var mapping = context.Item2;
        var key = AutomapId.FromDisk(i, mapping);
        if (save.Automaps.TryGetValue(key, out _))
            serdes.Bytes(null, save.Automaps[key], save.Automaps[key].Length);
        else if (serdes.IsReading())
            save.Automaps[key] = serdes.Bytes(null, null, size);
    }

    static void SerdesChest(int i, int size, (SavedGame, AssetMapping) context, ISerdes serdes)
    {
        var save = context.Item1;
        var mapping = context.Item2;
        var key = ChestId.FromDisk(i, mapping);
        Inventory existing = null;

        if (serdes.IsReading() || save.Inventories.TryGetValue(key, out existing))
            save.Inventories[key] = Inventory.SerdesChest(i, existing, mapping, serdes);
    }

    static void SerdesMerchant(int i, int size, (SavedGame, AssetMapping) context, ISerdes serdes)
    {
        if (i > 0xff)
            return;

        var save = context.Item1;
        var mapping = context.Item2;
        var key = MerchantId.FromDisk(i, mapping);
        Inventory existing = null;

        if (serdes.IsReading() || save.Inventories.TryGetValue(key, out existing))
            save.Inventories[key] = Inventory.SerdesMerchant(i, existing, mapping, serdes);
    }

    public bool IsEventUsed(AssetId eventSetId, ActionEvent action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var visited = new VisitedEvent(eventSetId, action.ActionType, action.Argument);
        return _visitedSet.Contains(visited);
    }

    public void UseEvent(AssetId eventSetId, ActionEvent action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var visited = new VisitedEvent(eventSetId, action.ActionType, action.Argument);
        if (_visitedSet.Add(visited))
            _visitedEvents.Add(visited);
    }

    // --- JSON Serialization ---

    static readonly JsonSerializerOptions JsonOptions = JsonSavedGame.JsonOptions;

    public JsonSavedGame ToJson(AssetMapping mapping, ISpellManager spellManager)
    {
        var json = new JsonSavedGame
        {
            Name = Name,
            Version = Version,
            ElapsedTime = FormatElapsedTime(ElapsedTime),
            MapId = mapping.IdToName(MapId),
            MapIdForNpcs = mapping.IdToName(MapIdForNpcs),
            PartyX = PartyX,
            PartyY = PartyY,
            PartyDirection = PartyDirection.ToString(),
            Unk0 = Unk0,
            MagicNumber = MagicNumber,
            Unk9 = Unk9,
            ActiveSpells = ActiveSpells,
            UnkB1 = UnkB1,
            Unk1A2 = Unk1A2,
            ActiveItems = (uint)ActiveItems,
            HoursSinceResting = HoursSinceResting,
            CombatPositions = CombatPositions?.Select(b => (int)b).ToArray(),
            Unknown5B8C = Unknown5B8C,
            Unknown8Bb8 = Unknown8Bb8,
            Switches = _switches.ActiveFlagIndices.ToArray(),
            UnlockedChests = _unlockedChests.ActiveFlagIndices.ToArray(),
            UnlockedDoors = _unlockedDoors.ActiveFlagIndices.ToArray(),
            AutomapMarkersFound = _automapMarkersFound.ActiveFlagIndices.ToArray(),
            Tickers = _tickers.Where(kv => kv.Value != 0).ToDictionary(kv => (int)kv.Key.Id, kv => kv.Value),
            Misc = JsonSerializer.SerializeToElement(new Dictionary<string, long>
            {
                [nameof(MiscState.Unk0)] = Misc.Unk0,
                [nameof(MiscState.Unk8)] = Misc.Unk8,
                [nameof(MiscState.Unk10)] = Misc.Unk10,
                [nameof(MiscState.Unk18)] = Misc.Unk18,
                [nameof(MiscState.Unk20)] = Misc.Unk20,
                [nameof(MiscState.Unk28)] = Misc.Unk28,
                [nameof(MiscState.Unk30)] = Misc.Unk30,
                [nameof(MiscState.Unk38)] = Misc.Unk38,
                [nameof(MiscState.Unk40)] = Misc.Unk40,
                [nameof(MiscState.Unk48)] = Misc.Unk48,
                [nameof(MiscState.Unk50)] = Misc.Unk50,
                [nameof(MiscState.Unk58)] = Misc.Unk58,
                [nameof(MiscState.Unk60)] = Misc.Unk60,
                [nameof(MiscState.Unk68)] = Misc.Unk68,
                [nameof(MiscState.Unk70)] = Misc.Unk70,
                [nameof(MiscState.Unk78)] = Misc.Unk78,
                [nameof(MiscState.Unk80)] = Misc.Unk80,
                [nameof(MiscState.Unk88)] = Misc.Unk88,
                [nameof(MiscState.Unk90)] = Misc.Unk90,
                [nameof(MiscState.Unk98)] = Misc.Unk98,
                [nameof(MiscState.UnkA0)] = Misc.UnkA0,
                [nameof(MiscState.UnkA8)] = Misc.UnkA8,
                [nameof(MiscState.UnkB0)] = Misc.UnkB0,
                [nameof(MiscState.UnkB8)] = Misc.UnkB8,
            }),
            Npcs = Npcs.Where(n => n != null).Select(n => ToJsonNpc(n, mapping)).ToList(),
            PermanentMapChanges = PermanentMapChanges.Select(mc => ToJsonMapChange(mc, mapping)).ToList(),
            TemporaryMapChanges = TemporaryMapChanges.Select(mc => ToJsonMapChange(mc, mapping)).ToList(),
            VisitedEvents = _visitedEvents.Select(ve => ToJsonVisitedEvent(ve, mapping)).ToList(),
            ActiveMembers = ActiveMembers.Select(m => m.IsNone ? null : m.ToString()).ToList(),
            SheetsBlob = SerializeSheetsBlobs(mapping, spellManager),
            InventoriesBlob = SerializeInventoriesBlobs(mapping),
            AutomapsBlob = Automaps.ToDictionary(kv => kv.Key.Id.ToString(), kv => Convert.ToBase64String(kv.Value)),
            PartyMembers = new List<JsonPartyMemberInfo>(),
        };

        json.RemovedNpcs = BuildMapScopedFlags(_removedNpcs, 512, 96);
        json.DisabledChains = BuildMapScopedFlags(_disabledChains, 512, 250);

        foreach (var memberId in ActiveMembers)
        {
            if (memberId.IsNone) continue;
            var sheetId = memberId.ToSheet();
            if (Sheets.TryGetValue(sheetId, out var sheet))
                json.PartyMembers.Add(new JsonPartyMemberInfo { SheetId = mapping.IdToName(sheetId), Name = sheet.EnglishName ?? sheet.PlayerClass.ToString(), Level = sheet.Level, ExperiencePoints = sheet.Combat?.ExperiencePoints ?? 0, PortraitId = (ushort)sheet.PortraitId.Id });
        }

        return json;
    }

    Dictionary<string, string> SerializeSheetsBlobs(AssetMapping mapping, ISpellManager spellManager)
    {
        var result = new Dictionary<string, string>();
        foreach (var (sheetId, sheet) in Sheets)
        {
            using var ms = new MemoryStream();
            using var w = AlbionSerdes.CreateWriter(ms);
            var context = (this, mapping, spellManager);
            SerdesSheet(sheetId, sheet, context, w);
            result[mapping.IdToName(sheetId)] = Convert.ToBase64String(ms.ToArray());
        }
        return result;
    }

    Dictionary<string, string> SerializeInventoriesBlobs(AssetMapping mapping)
    {
        var result = new Dictionary<string, string>();
        foreach (var (assetId, inv) in Inventories)
        {
            if (assetId.Type is not (AssetType.Chest or AssetType.Merchant))
                continue;
            using var ms = new MemoryStream();
            using var w = AlbionSerdes.CreateWriter(ms);
            var context = (this, mapping);
            SerdesInventory(assetId, inv, context, w);
            result[mapping.IdToName(assetId)] = Convert.ToBase64String(ms.ToArray());
        }
        return result;
    }

    static Dictionary<string, int[]> BuildMapScopedFlags(FlagSet flags, int mapCount, int bitsPerMap)
    {
        var result = new Dictionary<string, int[]>();
        for (int mapId = 0; mapId < mapCount; mapId++)
        {
            var indices = new List<int>();
            for (int i = 0; i < bitsPerMap; i++)
                if (flags.GetFlag(new MapId(mapId), i))
                    indices.Add(i);
            if (indices.Count > 0)
                result[new MapId(mapId).ToString()] = indices.ToArray();
        }
        return result;
    }

    public static SavedGame FromJson(string jsonString, AssetMapping mapping, ISpellManager spellManager)
    {
        var json = JsonSerializer.Deserialize<JsonSavedGame>(jsonString, JsonOptions);
        if (json == null) return null;

        var save = new SavedGame
        {
            Name = json.Name,
            Version = json.Version,
            ElapsedTime = ParseElapsedTime(json.ElapsedTime),
            MapId = ParseMapId(json.MapId, mapping),
            MapIdForNpcs = ParseMapId(json.MapIdForNpcs, mapping),
            PartyX = json.PartyX,
            PartyY = json.PartyY,
            PartyDirection = Enum.Parse<Direction>(json.PartyDirection),
            Unk0 = json.Unk0,
            MagicNumber = json.MagicNumber,
            Unk9 = json.Unk9,
            ActiveSpells = json.ActiveSpells ?? new ushort[0x50],
            UnkB1 = json.UnkB1 ?? new byte[0xE5],
            Unk1A2 = json.Unk1A2,
            ActiveItems = (ActiveItems)json.ActiveItems,
            HoursSinceResting = json.HoursSinceResting,
            CombatPositions = json.CombatPositions?.Select(i => (byte)i).ToArray() ?? new byte[MaxPartySize],
            Unknown5B8C = json.Unknown5B8C ?? new byte[0x2C],
            Unknown8Bb8 = json.Unknown8Bb8 ?? new byte[0x8c0],
        };

        foreach (var idx in json.Switches ?? Array.Empty<int>()) save._switches.SetFlag(idx, true);
        foreach (var idx in json.UnlockedChests ?? Array.Empty<int>()) save._unlockedChests.SetFlag(idx, true);
        foreach (var idx in json.UnlockedDoors ?? Array.Empty<int>()) save._unlockedDoors.SetFlag(idx, true);
        foreach (var idx in json.AutomapMarkersFound ?? Array.Empty<int>()) save._automapMarkersFound.SetFlag(idx, true);

        if (json.RemovedNpcs != null)
            foreach (var (mapName, indices) in json.RemovedNpcs)
                foreach (var idx in indices) save._removedNpcs.SetFlag(ParseMapId(mapName, mapping), idx, true);
        if (json.DisabledChains != null)
            foreach (var (mapName, indices) in json.DisabledChains)
                foreach (var idx in indices) save._disabledChains.SetFlag(ParseMapId(mapName, mapping), idx, true);

        if (json.Tickers != null)
            foreach (var (key, value) in json.Tickers)
                save._tickers[new TickerId(key)] = value;

        if (json.Misc.HasValue)
        {
            var miscEl = json.Misc.Value;
            if (miscEl.ValueKind == JsonValueKind.Object)
            {
                long Get(string k) => miscEl.TryGetProperty(k, out var v) && v.TryGetInt64(out var l) ? l : 0L;
                save.Misc = new MiscState { Unk0 = Get(nameof(MiscState.Unk0)), Unk8 = Get(nameof(MiscState.Unk8)), Unk10 = Get(nameof(MiscState.Unk10)), Unk18 = Get(nameof(MiscState.Unk18)), Unk20 = Get(nameof(MiscState.Unk20)), Unk28 = Get(nameof(MiscState.Unk28)), Unk30 = Get(nameof(MiscState.Unk30)), Unk38 = Get(nameof(MiscState.Unk38)), Unk40 = Get(nameof(MiscState.Unk40)), Unk48 = Get(nameof(MiscState.Unk48)), Unk50 = Get(nameof(MiscState.Unk50)), Unk58 = Get(nameof(MiscState.Unk58)), Unk60 = Get(nameof(MiscState.Unk60)), Unk68 = Get(nameof(MiscState.Unk68)), Unk70 = Get(nameof(MiscState.Unk70)), Unk78 = Get(nameof(MiscState.Unk78)), Unk80 = Get(nameof(MiscState.Unk80)), Unk88 = Get(nameof(MiscState.Unk88)), Unk90 = Get(nameof(MiscState.Unk90)), Unk98 = Get(nameof(MiscState.Unk98)), UnkA0 = Get(nameof(MiscState.UnkA0)), UnkA8 = Get(nameof(MiscState.UnkA8)), UnkB0 = Get(nameof(MiscState.UnkB0)), UnkB8 = Get(nameof(MiscState.UnkB8)) };
            }
            else if (miscEl.ValueKind == JsonValueKind.Array)
            {
                // Legacy format: flat array of 24 longs in declaration order
                var arr = miscEl.EnumerateArray().Select(e => e.TryGetInt64(out var v) ? v : 0L).ToArray();
                long At(int i) => i < arr.Length ? arr[i] : 0L;
                save.Misc = new MiscState { Unk0 = At(0), Unk8 = At(1), Unk10 = At(2), Unk18 = At(3), Unk20 = At(4), Unk28 = At(5), Unk30 = At(6), Unk38 = At(7), Unk40 = At(8), Unk48 = At(9), Unk50 = At(10), Unk58 = At(11), Unk60 = At(12), Unk68 = At(13), Unk70 = At(14), Unk78 = At(15), Unk80 = At(16), Unk88 = At(17), Unk90 = At(18), Unk98 = At(19), UnkA0 = At(20), UnkA8 = At(21), UnkB0 = At(22), UnkB8 = At(23) };
            }
        }

        if (json.Npcs != null)
            for (int i = 0; i < json.Npcs.Count && i < NpcCountPerMap; i++)
            {
                save.Npcs[i] ??= new NpcState();
                FromJsonNpc(json.Npcs[i], save.Npcs[i], mapping);
            }

        if (json.PermanentMapChanges != null)
            foreach (var mc in json.PermanentMapChanges) save.PermanentMapChanges.Add(FromJsonMapChange(mc, mapping));
        if (json.TemporaryMapChanges != null)
            foreach (var mc in json.TemporaryMapChanges) save.TemporaryMapChanges.Add(FromJsonMapChange(mc, mapping));

        if (json.VisitedEvents != null)
            foreach (var ve in json.VisitedEvents)
            {
                var evt = FromJsonVisitedEvent(ve, mapping);
                if (save._visitedSet.Add(evt)) save._visitedEvents.Add(evt);
            }

        if (json.ActiveMembers != null)
            for (int i = 0; i < MaxPartySize; i++)
                save.ActiveMembers[i] = i < json.ActiveMembers.Count && !string.IsNullOrEmpty(json.ActiveMembers[i]) ? PartyMemberId.Parse(json.ActiveMembers[i]) : PartyMemberId.None;

        if (json.SheetsBlob != null)
            foreach (var (name, blob) in json.SheetsBlob)
            {
                var sheetId = ParseSheetId(name, mapping);
                var bytes = Convert.FromBase64String(blob);
                using var ms = new MemoryStream(bytes);
                using var r = AlbionSerdes.CreateReader(ms);
                var context = (save, mapping, spellManager);

                var existing = save.Sheets.TryGetValue(sheetId, out var s) ? s : null;
                save.Sheets[sheetId] = SerdesSheet(sheetId, existing, context, r);
            }

        if (json.InventoriesBlob != null)
            foreach (var (name, blob) in json.InventoriesBlob)
            {
                var assetId = mapping.Parse(name, null);
                if (assetId.Type is not (AssetType.Chest or AssetType.Merchant))
                    continue;
                var bytes = Convert.FromBase64String(blob);
                using var ms = new MemoryStream(bytes);
                using var r = AlbionSerdes.CreateReader(ms);
                var context = (save, mapping);
                save.Inventories[assetId] = SerdesInventory(assetId, null, context, r);
            }

        if (json.AutomapsBlob != null)
            foreach (var (name, blob) in json.AutomapsBlob)
                if (int.TryParse(name, out var autoNum))
                    save.Automaps[AutomapId.FromDisk(autoNum, mapping)] = Convert.FromBase64String(blob);

        return save;
    }

    static MapId ParseMapId(string name, AssetMapping mapping) => name == null ? MapId.None : (MapId)mapping.Parse(name, MapId.ValidTypes);
    static SheetId ParseSheetId(string name, AssetMapping mapping) => name == null ? SheetId.None : (SheetId)mapping.Parse(name, SheetId.ValidTypes);

    static CharacterSheet SerdesSheet(SheetId id, CharacterSheet sheet, (SavedGame save, AssetMapping mapping, ISpellManager spellManager) context, ISerdes s) =>
        CharacterSheet.Serdes(id, sheet, context.mapping, s, context.spellManager);

    static Inventory SerdesInventory(AssetId id, Inventory inv, (SavedGame save, AssetMapping mapping) context, ISerdes s)
    {
        if (id.Type == AssetType.Chest)
            return Inventory.SerdesChest(id.Id, inv, context.mapping, s);
        if (id.Type == AssetType.Merchant)
            return Inventory.SerdesMerchant(id.Id, inv, context.mapping, s);
        return inv;
    }

    // --- NPC conversion ---
    static JsonNpcState ToJsonNpc(NpcState npc, AssetMapping mapping)
    {
        return new JsonNpcState
        {
            Id = npc.Id.IsNone ? null : mapping.IdToName(npc.Id),
            SpriteOrGroup = npc.SpriteOrGroup.IsNone ? null : mapping.IdToName(npc.SpriteOrGroup),
            Type = npc.Type.ToString(),
            NoClip = npc.NoClip,
            Sound = npc.Sound,
            ActiveSfx0 = npc.ActiveSfx0,
            ActiveSfx1 = npc.ActiveSfx1,
            ActiveSfx2 = npc.ActiveSfx2,
            ActiveSfx3 = npc.ActiveSfx3,
            Triggers = npc.Triggers.ToString(),
            EventIndex = npc.EventIndex,
            MovementType = npc.MovementType.ToString(),
            WasActive = npc.WasActive,
            Flags = npc.Flags.ToString(),
            Unk1A = npc.Unk1A,
            Unk1B = npc.Unk1B,
            Unk1D = npc.Unk1D,
            WaypointDataOffset = npc.WaypointDataOffset,
            Unk23 = npc.Unk23,
            Angle = npc.Angle,
            WaypointIndex = npc.WaypointIndex,
            Unk29 = npc.Unk29,
            X = npc.X,
            Y = npc.Y,
            X2 = npc.X2,
            Y2 = npc.Y2,
            PixelX = npc.PixelX,
            PixelY = npc.PixelY,
            PixelDeltaX = npc.PixelDeltaX,
            PixelDeltaY = npc.PixelDeltaY,
            Unk42 = npc.Unk42,
            OldX = npc.OldX,
            OldY = npc.OldY,
            MoveToX = npc.MoveToX,
            MoveToY = npc.MoveToY,
            Unk4C = npc.Unk4C,
            Unk4E = npc.Unk4E,
            Unk50 = npc.Unk50,
            Unk51 = npc.Unk51,
            Unk52 = npc.Unk52,
            Unk53 = npc.Unk53,
            Unk54 = npc.Unk54,
            Unk56 = npc.Unk56,
            Unk58 = npc.Unk58,
            GfxWidth = npc.GfxWidth,
            GfxHeight = npc.GfxHeight,
            Unk5EGfxRelated = npc.Unk5EGfxRelated,
            GfxAlloc = npc.GfxAlloc,
            Unk64 = npc.Unk64,
            Unk65 = npc.Unk65,
            Unk66 = npc.Unk66,
            MoveState = ToJsonNpcMoveState(npc.NpcMoveState),
        };
    }

    static JsonNpcMoveState ToJsonNpcMoveState(NpcMoveState ms)
    {
        return new JsonNpcMoveState
        {
            Flags = ms.Flags,
            X1 = ms.X1,
            Y1 = ms.Y1,
            Angle1 = ms.Angle1,
            X2 = ms.X2,
            Y2 = ms.Y2,
            Direction = ms.Direction.ToString(),
            UnkE = ms.UnkE,
            Unk10 = ms.Unk10,
            Unk12 = ms.Unk12,
            Unk14 = ms.Unk14,
            Unk16 = ms.Unk16,
        };
    }

    static void FromJsonNpc(JsonNpcState json, NpcState npc, AssetMapping mapping)
    {
        npc.Id = json.Id != null ? AssetId.Parse(json.Id) : AssetId.None;
        npc.SpriteOrGroup = json.SpriteOrGroup != null ? AssetId.Parse(json.SpriteOrGroup) : AssetId.None;
        npc.Type = Enum.Parse<NpcType>(json.Type);
        npc.NoClip = json.NoClip;
        npc.Sound = json.Sound;
        npc.ActiveSfx0 = json.ActiveSfx0;
        npc.ActiveSfx1 = json.ActiveSfx1;
        npc.ActiveSfx2 = json.ActiveSfx2;
        npc.ActiveSfx3 = json.ActiveSfx3;
        npc.Triggers = Enum.Parse<TriggerTypes>(json.Triggers);
        npc.EventIndex = json.EventIndex;
        npc.MovementType = Enum.Parse<NpcMovement>(json.MovementType);
        npc.WasActive = json.WasActive;
        npc.Flags = Enum.Parse<NpcFlags>(json.Flags);
        npc.Unk1A = json.Unk1A;
        npc.Unk1B = json.Unk1B;
        npc.Unk1D = json.Unk1D;
        npc.WaypointDataOffset = json.WaypointDataOffset;
        npc.Unk23 = json.Unk23;
        npc.Angle = json.Angle;
        npc.WaypointIndex = json.WaypointIndex;
        npc.Unk29 = json.Unk29;
        npc.X = json.X;
        npc.Y = json.Y;
        npc.X2 = json.X2;
        npc.Y2 = json.Y2;
        npc.PixelX = json.PixelX;
        npc.PixelY = json.PixelY;
        npc.PixelDeltaX = json.PixelDeltaX;
        npc.PixelDeltaY = json.PixelDeltaY;
        npc.Unk42 = json.Unk42;
        npc.OldX = json.OldX;
        npc.OldY = json.OldY;
        npc.MoveToX = json.MoveToX;
        npc.MoveToY = json.MoveToY;
        npc.Unk4C = json.Unk4C;
        npc.Unk4E = json.Unk4E;
        npc.Unk50 = json.Unk50;
        npc.Unk51 = json.Unk51;
        npc.Unk52 = json.Unk52;
        npc.Unk53 = json.Unk53;
        npc.Unk54 = json.Unk54;
        npc.Unk56 = json.Unk56;
        npc.Unk58 = json.Unk58;
        npc.GfxWidth = json.GfxWidth;
        npc.GfxHeight = json.GfxHeight;
        npc.Unk5EGfxRelated = json.Unk5EGfxRelated;
        npc.GfxAlloc = json.GfxAlloc;
        npc.Unk64 = json.Unk64;
        npc.Unk65 = json.Unk65;
        npc.Unk66 = json.Unk66;
        if (json.MoveState != null)
            FromJsonNpcMoveState(json.MoveState, npc.NpcMoveState);
    }

    static void FromJsonNpcMoveState(JsonNpcMoveState json, NpcMoveState ms)
    {
        ms.Flags = json.Flags;
        ms.X1 = json.X1;
        ms.Y1 = json.Y1;
        ms.Angle1 = json.Angle1;
        ms.X2 = json.X2;
        ms.Y2 = json.Y2;
        ms.Direction = Enum.Parse<Direction>(json.Direction);
        ms.UnkE = json.UnkE;
        ms.Unk10 = json.Unk10;
        ms.Unk12 = json.Unk12;
        ms.Unk14 = json.Unk14;
        ms.Unk16 = json.Unk16;
    }

    // --- MapChange conversion ---
    static JsonMapChange ToJsonMapChange(MapChange mc, AssetMapping mapping)
    {
        return new JsonMapChange
        {
            MapId = mapping.IdToName(mc.MapId),
            Layers = mc.Layers.ToString(),
            ChangeType = mc.ChangeType.ToString(),
            X = mc.X,
            Y = mc.Y,
            Value = mc.Value,
        };
    }

    static MapChange FromJsonMapChange(JsonMapChange json, AssetMapping mapping)
    {
        return new MapChange
        {
            MapId = ParseMapId(json.MapId, mapping),
            Layers = Enum.Parse<ChangeIconLayers>(json.Layers),
            ChangeType = Enum.Parse<IconChangeType>(json.ChangeType),
            X = json.X,
            Y = json.Y,
            Value = json.Value,
        };
    }

    // --- VisitedEvent conversion ---
    static JsonVisitedEvent ToJsonVisitedEvent(VisitedEvent ve, AssetMapping mapping)
    {
        return new JsonVisitedEvent
        {
            EventSetId = mapping.IdToName(ve.EventSetId),
            ActionType = ve.Type.ToString(),
            Argument = mapping.IdToName(ve.Argument),
        };
    }

    static VisitedEvent FromJsonVisitedEvent(JsonVisitedEvent json, AssetMapping mapping)
    {
        var actionType = Enum.Parse<ActionType>(json.ActionType);
        var argument = mapping.Parse(json.Argument, new[] { actionType.GetAssetType() });
        var eventSetId = mapping.Parse(json.EventSetId, EventSetId.ValidTypes);
        return new VisitedEvent(eventSetId, actionType, argument);
    }

    static string FormatElapsedTime(TimeSpan ts) =>
        ts.TotalDays >= 1 ? $"{ts.Days}.{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}" : $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

    static TimeSpan ParseElapsedTime(string s)
    {
        if (string.IsNullOrEmpty(s)) return TimeSpan.Zero;
        var parts = s.Split('.');
        if (parts.Length == 2 && int.TryParse(parts[0], out var days))
        {
            var timeParts = parts[1].Split(':');
            if (timeParts.Length >= 3 &&
                int.TryParse(timeParts[0], out var h) &&
                int.TryParse(timeParts[1], out var m) &&
                int.TryParse(timeParts[2], out var sec))
                return new TimeSpan(days, h, m, sec);
        }
        return TimeSpan.TryParse(s, out var result) ? result : TimeSpan.Zero;
    }
}
