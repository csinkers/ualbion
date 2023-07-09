using System.Text.Json.Serialization;

namespace UAlbion.Config;

/// <summary>
/// An asset type corresponds to a particular type of asset object intended for a particular purpose.
/// e.g. a character sheet asset intended for use as a party member has a different AssetType to an NPC's character sheet.
/// If this enum is modified, then CodeGeneration must be re-run.
/// Any AssetTypes in a family should have their own AssetId type, in additional to any more general id types they may be a part of.
/// </summary>
[JsonConverter(typeof(ToStringJsonConverter<AssetType>))]
public enum AssetType : byte
{
    [Unmapped] None = 0, // Must be 0 so default(AssetId) will equate to AssetId.None

    // Various kinds of SpriteId
    AutomapGfx,
    BackgroundGfx,
    CombatBackground,
    CombatGfx,
    CoreGfx,
    Floor,
    FontGfx,
    ItemGfx,
    MonsterGfx,
    NpcLargeGfx,
    NpcSmallGfx,
    Object3D,
    [IsomorphicTo(PartyMember)] PartyInventoryGfx,
    [IsomorphicTo(PartyMember)] PartyLargeGfx,
    [IsomorphicTo(PartyMember)] PartySmallGfx,
    Picture,
    Portrait,
    Slab,
    TacticalGfx,
    [IsomorphicTo(Tileset)] TilesetGfx,
    Wall,
    WallOverlay,

    // Text rendering
    Ink,
    FontDefinition,
    [Unmapped] MetaFont,

    // Environmental objects that are setup on new game and have their state tracked in the save files
    Chest,
    Merchant,
    Door,

    // Scripts
    EventSet,
    Script,

    // Inventory items
    [Unmapped] Gold,
    [Unmapped] Rations,
    Item,

    // 3D template
    Labyrinth,

    // Actual map data
    Map,
    [IsomorphicTo(Map)] Automap,

    // Character definitions
    PartyMember,
    [IsomorphicTo(PartyMember)] PartySheet,
    MonsterSheet,
    NpcSheet,

    // Misc
    MonsterGroup,
    [Unmapped] ObjectGroup,
    Palette,
    Sample,
    Song,
    WaveLibrary,
    Spell,
    Switch,
    Ticker,
    Tileset,
    [IsomorphicTo(Tileset)] BlockList,
    [Localised] Video,

    // Individual strings
    [Localised] [IsomorphicTo(Item)] ItemName,
    [Localised] Text,
    [Localised] Word,

    // String sets
    [Localised] [IsomorphicTo(EventSet)] EventText,
    [Localised] [IsomorphicTo(Map)] MapText,

    Special, // For various types that only have a single value, can be a string set

    [Unmapped] MapTextIndex, // Used for NPCs with the SimpleMsg flag
    [Unmapped] PromptNumber, // Used for DialogueLine actions
    [Unmapped] LocalNpc, // For identifying NPCs in a map by their slot number
    Target, // For targeting DataChangeEvents etc

    [Unmapped] Unknown = 255
}