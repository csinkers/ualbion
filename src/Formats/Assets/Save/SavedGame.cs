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

namespace UAlbion.Formats.Assets.Save;

public class SavedGame
{
    public const int MaxPartySize = 6;
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

    readonly FlagSet _switches  = new(0, SwitchCount);
    readonly FlagSet _unlockedChests  = new(0, ChestCount);
    readonly FlagSet _unlockedDoors  = new(0, DoorCount);
    readonly FlagSet _removedNpcs  = new(0, MapCount * NpcCountPerMap, NpcCountPerMap);
    readonly FlagSet _disabledChains  = new(0, MapCount * ChainCountPerMap, ChainCountPerMap);
    readonly FlagSet _automapMarkersFound = new(0, AutomapMarkerCount);
    readonly TickerSet _tickers = new();

    public IDictionary<TickerId, byte> Tickers => _tickers;
    public bool GetFlag(SwitchId flag) => _switches.GetFlag(flag.Id);
    public void SetFlag(SwitchId flag, bool value) => _switches.SetFlag(flag.Id, value);
    public bool IsNpcDisabled(MapId mapId, int npcNumber)
    {
        if (mapId.IsNone)
            mapId = MapId;

        // TODO: Check for possible off-by-one
        return npcNumber is < 0 or >= NpcCountPerMap
               || mapId.Id is < 0 or >= MapCount
               || _removedNpcs.GetFlag(mapId.Id * NpcCountPerMap + npcNumber);
    }

    public void SetNpcDisabled(MapId mapId, int npcNumber, bool isDisabled)
    {
        if (mapId.IsNone)
            mapId = MapId;

        if (npcNumber is < 0 or >= NpcCountPerMap)
            return;

        _removedNpcs.SetFlag(mapId.Id * NpcCountPerMap + npcNumber, isDisabled);
    }

    public bool IsChainDisabled(MapId mapId, int chainNumber)
    {
        if (mapId.IsNone)
            mapId = MapId;

        // TODO: Check for possible off-by-one
        return chainNumber is < 0 or >= ChainCountPerMap
               || mapId.Id is < 0 or >= MapCount
               || _disabledChains.GetFlag(mapId.Id * ChainCountPerMap + chainNumber);
    }

    public void SetChainDisabled(MapId mapId, int chainNumber, bool isDisabled)
    {
        if (mapId.IsNone)
            mapId = MapId;

        if (chainNumber is < 0 or >= ChainCountPerMap)
            return;

        _disabledChains.SetFlag(mapId.Id * ChainCountPerMap + chainNumber, isDisabled);
    }

    public ushort Unk0 { get; set; }
    public uint MagicNumber { get; set; }
    public uint Unk9 { get; set; }
    public ushort[] ActiveSpells { get; set; }
    public byte[] UnkB5 { get; set; }
    public MiscState Misc { get; private set; } = new();
    public byte[] Unknown5B90 { get; set; }
    public NpcState[] Npcs { get; } = new NpcState[NpcCountPerMap];
    public byte[] Unknown5B71 { get; set; } 
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
        ApiUtil.Assert(save.MagicNumber == 0x25051971, "Magic number was expected to be 0x25051971"); // Must be someone's birthday
        save.Version = s.UInt32(nameof(Version), save.Version);
        ApiUtil.Assert(save.Version == 138); // TODO: Throw error for other versions?

        // ------------------------------
        // ---- START OF GAME HEADER ----
        // ------------------------------
        var headerOffset = s.Offset;
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

        save.UnkB5 = s.Bytes(nameof(UnkB5), save.UnkB5, 0xE5);

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

        save.Misc = s.Object(nameof(Misc), save.Misc, MiscState.Serdes); // 1A2
        save._switches.Serdes("Switches", s); // 272

        // save._unk5Flags.Serdes("Unk5Flags", s);
        // save._unk6Flags.Serdes("Unk6Flags", s);
        // save._unk8Flags.Serdes("Unk8Flags", s);

        // TODO: Chain, Door, Chest, Npc, KnownWord flag dictionaries. Known 3D automap info markers? Battle positions?
        save._disabledChains.Serdes("DisabledChains", s);
        save._removedNpcs.Serdes("RemovedNpcs", s); // 2f6 + 3e80 = 4176
        save._automapMarkersFound.Serdes("AutomapMarkers", s); // 5972
        save._unlockedChests.Serdes("UnlockedChests", s); // 5992
        save._unlockedDoors.Serdes("UnlockedDoors", s); // 5A0E
        s.Object(nameof(Tickers), save._tickers, TickerSet.Serdes); // 5A8C

        // ----------------------------
        // ---- END OF GAME HEADER ----
        // ----------------------------

        ApiUtil.Assert(s.Offset - headerOffset == 0x5b8e);
        save.Unknown5B90 = s.Bytes(nameof(Unknown5B90), save.Unknown5B90, 0x2C);
        var mapType = MapType.TwoD;
        s.List(nameof(save.Npcs), save.Npcs, (mapType, mapping), NpcCountPerMap, NpcState.Serdes);

        save.Unknown5B71 = s.Bytes( nameof(Unknown5B71), save.Unknown5B71, 0x8c0);

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
}
