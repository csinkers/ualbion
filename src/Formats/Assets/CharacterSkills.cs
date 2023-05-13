using System;
namespace UAlbion.Formats.Assets;

public interface ICharacterSkills
{
    ICharacterAttribute CloseCombat { get; }
    ICharacterAttribute RangedCombat { get; }
    ICharacterAttribute CriticalChance { get; }
    ICharacterAttribute LockPicking { get; }
}

public class CharacterSkills : ICharacterSkills
{
    public override string ToString() => $"M{CloseCombat} R{RangedCombat} C{CriticalChance} L{LockPicking}";
    ICharacterAttribute ICharacterSkills.CloseCombat => CloseCombat;
    ICharacterAttribute ICharacterSkills.RangedCombat => RangedCombat;
    ICharacterAttribute ICharacterSkills.CriticalChance => CriticalChance;
    ICharacterAttribute ICharacterSkills.LockPicking => LockPicking;
    public CharacterAttribute CloseCombat { get; set; }
    public CharacterAttribute RangedCombat { get; set; }
    public CharacterAttribute CriticalChance { get; set; }
    public CharacterAttribute LockPicking { get; set; }
    public CharacterSkills DeepClone() => new CharacterSkills().CopyFrom(this);
    public CharacterSkills CopyFrom(CharacterSkills other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        CloseCombat = other.CloseCombat.DeepClone();
        RangedCombat = other.RangedCombat.DeepClone();
        CriticalChance = other.CriticalChance.DeepClone();
        LockPicking = other.LockPicking.DeepClone();
        return this;
    }
}