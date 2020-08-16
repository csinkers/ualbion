using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public interface ICharacterSheet
    {
        AssetKey Key { get; }
        string GetName(GameLanguage language);

        CharacterType Type { get; }
        Gender Gender { get; }
        PlayerRaces Races { get; }
        PlayerClass PlayerClass { get; }
        ushort Age { get; }
        byte Level { get; }

        AssetId SpriteId { get; }
        SmallPortraitId? PortraitId { get; }
        EventSetId? EventSetId { get; }
        EventSetId? WordSetId { get; }
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
}
