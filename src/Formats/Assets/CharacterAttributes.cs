using System;
using System.Collections.Generic;

namespace UAlbion.Formats.Assets;

public class CharacterAttributes : ICharacterAttributes
{
    public IEnumerable<CharacterAttribute> Enumerate()
    {
        yield return Strength;
        yield return Intelligence;
        yield return Dexterity;
        yield return Speed;
        yield return Stamina;
        yield return Luck;
        yield return MagicResistance;
        yield return MagicTalent;
    }

    public override string ToString() => $"S{Strength} I{Intelligence} D{Dexterity} Sp{Speed} St{Stamina} L{Luck} MR{MagicResistance} MT{MagicTalent}";

    ICharacterAttribute ICharacterAttributes.Strength => Strength;
    ICharacterAttribute ICharacterAttributes.Intelligence => Intelligence;
    ICharacterAttribute ICharacterAttributes.Dexterity => Dexterity;
    ICharacterAttribute ICharacterAttributes.Speed => Speed;
    ICharacterAttribute ICharacterAttributes.Stamina => Stamina;
    ICharacterAttribute ICharacterAttributes.Luck => Luck;
    ICharacterAttribute ICharacterAttributes.MagicResistance => MagicResistance;
    ICharacterAttribute ICharacterAttributes.MagicTalent => MagicTalent;
    public CharacterAttribute Strength { get; set; }
    public CharacterAttribute Intelligence { get; set; }
    public CharacterAttribute Dexterity { get; set; }
    public CharacterAttribute Speed { get; set; }
    public CharacterAttribute Stamina { get; set; }
    public CharacterAttribute Luck { get; set; }
    public CharacterAttribute MagicResistance { get; set; }
    public CharacterAttribute MagicTalent { get; set; }

    public CharacterAttributes DeepClone() => new CharacterAttributes().CopyFrom(this);
    public CharacterAttributes CopyFrom(CharacterAttributes other)
    {
        ArgumentNullException.ThrowIfNull(other);
        Strength = other.Strength.DeepClone();
        Intelligence = other.Intelligence.DeepClone();
        Dexterity = other.Dexterity.DeepClone();
        Speed = other.Speed.DeepClone();
        Stamina = other.Stamina.DeepClone();
        Luck = other.Luck.DeepClone();
        MagicResistance = other.MagicResistance.DeepClone();
        MagicTalent = other.MagicTalent.DeepClone();
        return this;
    }
}