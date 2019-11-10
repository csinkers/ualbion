using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public interface ICharacterSheet
    {
        string Name { get; } // Debug name, not displayed to the player
        string GetName(GameLanguage language);

        CharacterType Type { get; }
        Gender Gender { get; }
        PlayerRace Race { get; }
        PlayerClass Class { get; }
        ushort Age { get; }
        byte Level { get; }

        byte SpriteId { get; }
        AssetType SpriteType { get; }
        SmallPortraitId? PortraitId { get; }
        ushort EventSetId { get; }
        ushort WordSet { get; }
        PlayerLanguage Languages { get; }

        // Grouped
        IMagicSkills Magic { get; }
        ICharacterInventory Inventory { get; }
        ICharacterAttributes Attributes { get; }
        ICharacterSkills Skills { get; }
        ICombatAttributes Combat { get; }

        ItemSlot GetSlot(ItemSlotId itemSlotId);
    }
}