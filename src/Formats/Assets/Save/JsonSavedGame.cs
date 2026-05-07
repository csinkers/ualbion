using System.Collections.Generic;
using System.Text.Json;

namespace UAlbion.Formats.Assets.Save;

/// <summary>
/// Vollständiges JSON-DTO für Save-Games. Die meisten State-Daten direkt serialisiert.
/// Sheets/Inventories/Automaps nutzen Base64 (zu komplex für flaches JSON).
/// </summary>
public class JsonSavedGame
{
    public static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };


    // --- Header ---
    public string Name { get; set; }
    public uint Version { get; set; } = 138;
    public string ElapsedTime { get; set; }
    public string MapId { get; set; }
    public string MapIdForNpcs { get; set; }
    public ushort PartyX { get; set; }
    public ushort PartyY { get; set; }
    public string PartyDirection { get; set; }
    public ushort Unk0 { get; set; }
    public uint MagicNumber { get; set; } = 0x25051971;
    public uint Unk9 { get; set; }

    // --- Primitive arrays ---
    public ushort[] ActiveSpells { get; set; }
    public byte[] UnkB1 { get; set; }
    public int Unk1A2 { get; set; }
    public uint ActiveItems { get; set; }
    public ushort HoursSinceResting { get; set; }
    public int[] CombatPositions { get; set; }
    // JsonElement? lets us tolerate both the old array format and the current dict format.
    public System.Text.Json.JsonElement? Misc { get; set; }
    public byte[] Unknown5B8C { get; set; }
    public byte[] Unknown8Bb8 { get; set; }

    // --- Flags (sparse: indices die true sind) ---
    public int[] Switches { get; set; }
    public int[] UnlockedChests { get; set; }
    public int[] UnlockedDoors { get; set; }
    public Dictionary<string, int[]> RemovedNpcs { get; set; }
    public Dictionary<string, int[]> DisabledChains { get; set; }
    public int[] AutomapMarkersFound { get; set; }

    // --- Tickers (nur non-zero entries) ---
    public Dictionary<int, byte> Tickers { get; set; }

    // --- NPCs ---
    public List<JsonNpcState> Npcs { get; set; } = [];

    // --- Map changes ---
    public List<JsonMapChange> PermanentMapChanges { get; set; } = [];
    public List<JsonMapChange> TemporaryMapChanges { get; set; } = [];

    // --- Visited events ---
    public List<JsonVisitedEvent> VisitedEvents { get; set; } = [];

    // --- Party ---
    public List<string> ActiveMembers { get; set; } = [];

    // --- Sheets: Base64 blobs (key = SheetId name) ---
    public Dictionary<string, string> SheetsBlob { get; set; } = [];

    // --- Inventories: Base64 blobs (key = AssetId name) ---
    public Dictionary<string, string> InventoriesBlob { get; set; } = [];

    // --- Automaps: Base64 blobs (key = AutomapId name) ---
    public Dictionary<string, string> AutomapsBlob { get; set; } = [];

    // --- UI Preview helpers ---
    public List<JsonPartyMemberInfo> PartyMembers { get; set; } = [];
}

// ─── NpcState DTO ──────────────────────────────────────────────────────────

public class JsonNpcState
{
    public string Id { get; set; }
    public string SpriteOrGroup { get; set; }
    public string Type { get; set; }
    public bool NoClip { get; set; }
    public ushort Sound { get; set; }
    public ushort ActiveSfx0 { get; set; }
    public ushort ActiveSfx1 { get; set; }
    public ushort ActiveSfx2 { get; set; }
    public ushort ActiveSfx3 { get; set; }
    public string Triggers { get; set; }
    public ushort EventIndex { get; set; }
    public string MovementType { get; set; }
    public ushort WasActive { get; set; }
    public string Flags { get; set; }
    public byte Unk1A { get; set; }
    public ushort Unk1B { get; set; }
    public ushort Unk1D { get; set; }
    public uint WaypointDataOffset { get; set; }
    public ushort Unk23 { get; set; }
    public ushort Angle { get; set; }
    public ushort WaypointIndex { get; set; }
    public byte Unk29 { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }
    public ushort X2 { get; set; }
    public ushort Y2 { get; set; }
    public float PixelX { get; set; }
    public float PixelY { get; set; }
    public int PixelDeltaX { get; set; }
    public int PixelDeltaY { get; set; }
    public ushort Unk42 { get; set; }
    public ushort OldX { get; set; }
    public ushort OldY { get; set; }
    public ushort MoveToX { get; set; }
    public ushort MoveToY { get; set; }
    public ushort Unk4C { get; set; }
    public ushort Unk4E { get; set; }
    public byte Unk50 { get; set; }
    public byte Unk51 { get; set; }
    public byte Unk52 { get; set; }
    public byte Unk53 { get; set; }
    public ushort Unk54 { get; set; }
    public ushort Unk56 { get; set; }
    public ushort Unk58 { get; set; }
    public ushort GfxWidth { get; set; }
    public ushort GfxHeight { get; set; }
    public ushort Unk5EGfxRelated { get; set; }
    public uint GfxAlloc { get; set; }
    public byte Unk64 { get; set; }
    public byte Unk65 { get; set; }
    public ushort Unk66 { get; set; }
    public JsonNpcMoveState MoveState { get; set; } = new();
}

public class JsonNpcMoveState
{
    public ushort Flags { get; set; }
    public ushort X1 { get; set; }
    public ushort Y1 { get; set; }
    public ushort Angle1 { get; set; }
    public ushort X2 { get; set; }
    public ushort Y2 { get; set; }
    public string Direction { get; set; }
    public ushort UnkE { get; set; }
    public ushort Unk10 { get; set; }
    public ushort Unk12 { get; set; }
    public ushort Unk14 { get; set; }
    public ushort Unk16 { get; set; }
}

// ─── MapChange DTO ─────────────────────────────────────────────────────────

public class JsonMapChange
{
    public string MapId { get; set; }
    public string Layers { get; set; }
    public string ChangeType { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    public ushort Value { get; set; }
}

// ─── VisitedEvent DTO ──────────────────────────────────────────────────────

public class JsonVisitedEvent
{
    public string EventSetId { get; set; }
    public string ActionType { get; set; }
    public string Argument { get; set; }
}

// ─── Party Member Preview ──────────────────────────────────────────────────

public class JsonPartyMemberInfo
{
    public string SheetId { get; set; }
    public string Name { get; set; }
    public byte Level { get; set; }
    public int ExperiencePoints { get; set; }
}
