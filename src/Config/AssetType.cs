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

    Ink,
    FontDefinition,
    [Unmapped] MetaFont,

    Chest,
    Merchant,
    Door,

    EventSet,
    Script,

    [Unmapped] Gold,
    [Unmapped] Rations,
    Item,

    Labyrinth,

    Map,
    [IsomorphicTo(Map)] Automap,

    PartyMember,
    [IsomorphicTo(PartyMember)] PartySheet,
    MonsterSheet,
    NpcSheet,

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

    [Localised] [IsomorphicTo(EventSet)] EventText,
    [Localised] [IsomorphicTo(Map)] MapText,
    [Localised] [IsomorphicTo(Item)] ItemName,
    [Localised] Text,
    [Localised] Word,

    [Unmapped] MapTextIndex = 252, // Used for NPCs with the SimpleMsg flag
    [Unmapped] PromptNumber = 251, // Used for DialogueLine actions
    [Unmapped] LocalNpc = 252, // For identifying NPCs in a map by their slot number
    Target = 253, // For targeting DataChangeEvents etc
    Special = 254, // For various types that only have a single value
    [Unmapped] Unknown = 255
}