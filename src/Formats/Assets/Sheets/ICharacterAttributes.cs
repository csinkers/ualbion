namespace UAlbion.Formats.Assets.Sheets;

public interface ICharacterAttributes
{
    ICharacterAttribute Strength { get; }
    ICharacterAttribute Intelligence { get; }
    ICharacterAttribute Dexterity { get; }
    ICharacterAttribute Speed { get; }
    ICharacterAttribute Stamina { get; }
    ICharacterAttribute Luck { get; }
    ICharacterAttribute MagicResistance { get; }
    ICharacterAttribute MagicTalent { get; }
}