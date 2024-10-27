namespace UAlbion.Formats.Assets.Sheets;

public interface ICharacterSkills
{
    ICharacterAttribute CloseCombat { get; }
    ICharacterAttribute RangedCombat { get; }
    ICharacterAttribute CriticalChance { get; }
    ICharacterAttribute LockPicking { get; }
}