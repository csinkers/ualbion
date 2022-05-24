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

    SpriteId SpriteId { get; }
    SpriteId PortraitId { get; }
    EventSetId EventSetId { get; }
    EventSetId WordSetId { get; }
    PlayerLanguages Languages { get; }

    // Grouped
    IMagicSkills Magic { get; }
    IInventory Inventory { get; }
    ICharacterAttributes Attributes { get; }
    ICharacterSkills Skills { get; }
    ICombatAttributes Combat { get; }
}

public interface IEffectiveCharacterSheet : ICharacterSheet
{
    int TotalWeight { get; }
    int MaxWeight { get; }
    int DisplayDamage { get; }
    int DisplayProtection { get; }
}
