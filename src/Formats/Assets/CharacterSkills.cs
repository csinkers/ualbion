using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.Assets;

public class CharacterSkills : ICharacterSkills
{
    public IEnumerable<CharacterAttribute> Enumerate()
    {
        yield return CloseCombat;
        yield return RangedCombat;
        yield return CriticalChance;
        yield return LockPicking;
    }

    public override string ToString() => $"M{CloseCombat} R{RangedCombat} C{CriticalChance} L{LockPicking}";

    ICharacterAttribute ICharacterSkills.CloseCombat => CloseCombat;
    ICharacterAttribute ICharacterSkills.RangedCombat => RangedCombat;
    ICharacterAttribute ICharacterSkills.CriticalChance => CriticalChance;
    ICharacterAttribute ICharacterSkills.LockPicking => LockPicking;

    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)] public CharacterAttribute CloseCombat { get; set; }
    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)] public CharacterAttribute RangedCombat { get; set; }
    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)] public CharacterAttribute CriticalChance { get; set; }
    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)] public CharacterAttribute LockPicking { get; set; }

    public CharacterSkills DeepClone() => new CharacterSkills().CopyFrom(this);
    public CharacterSkills CopyFrom(CharacterSkills other)
    {
        ArgumentNullException.ThrowIfNull(other);

        CloseCombat = other.CloseCombat.DeepClone();
        RangedCombat = other.RangedCombat.DeepClone();
        CriticalChance = other.CriticalChance.DeepClone();
        LockPicking = other.LockPicking.DeepClone();
        return this;
    }
}