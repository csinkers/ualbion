using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public interface ICharacterSheet
{
    SheetId Id { get; }
    string GetName(string language);

    CharacterType Type { get; }
    Gender Gender { get; }
    PlayerRace Race { get; }
    PlayerClass PlayerClass { get; }
    ICharacterAttribute Age { get; }
    byte Level { get; }

    // Visual
    SpriteId SpriteId { get; } // Overworld / 3D graphics
    SpriteId PortraitId { get; } // Conversation portrait
    SpriteId TwoDGfxId { get; } // Combat 3D graphics
    SpriteId TacticalGfxId { get; } // Combat 2D graphics

    EventSetId EventSetId { get; }
    EventSetId WordSetId { get; } // Base set of conversation topics
    PlayerLanguages Languages { get; }

    // Grouped
    IMagicSkills Magic { get; }
    IInventory Inventory { get; }
    ICharacterAttributes Attributes { get; }
    ICharacterSkills Skills { get; }
    ICombatAttributes Combat { get; }
}