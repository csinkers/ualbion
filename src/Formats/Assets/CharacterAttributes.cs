using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;

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

    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)] public CharacterAttribute Strength { get; set; }
    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)] public CharacterAttribute Intelligence { get; set; }
    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)] public CharacterAttribute Dexterity { get; set; }
    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)] public CharacterAttribute Speed { get; set; }
    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)] public CharacterAttribute Stamina { get; set; }
    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)] public CharacterAttribute Luck { get; set; }
    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)] public CharacterAttribute MagicResistance { get; set; }
    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)] public CharacterAttribute MagicTalent { get; set; }

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