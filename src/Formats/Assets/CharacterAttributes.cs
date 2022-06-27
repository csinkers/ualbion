using SerdesNet;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets;

public interface ICharacterAttribute
{
    ushort Current { get; }
    ushort Max { get; }
    ushort Boost { get; }
    ushort Backup { get; }
}

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

public class CharacterAttribute : ICharacterAttribute
{
    public ushort Current { get; set; }
    public ushort Max { get; set; }
    public ushort Boost { get; set; }
    public ushort Backup { get; set; }
    public CharacterAttribute DeepClone() => (CharacterAttribute)MemberwiseClone();
    public override string ToString() => $"[{Current}/{Max}]{(Boost > 0 ? $"+{Boost}" : "")}{(Backup > 0 ? $" (was {Backup})" : "")}";

    public static CharacterAttribute Serdes(string name, CharacterAttribute attr, ISerializer s, bool hasBackup = true)
    {
        s.Begin(name);
        attr ??= new CharacterAttribute();
        attr.Current = s.UInt16(nameof(Current), attr.Current);
        attr.Max = s.UInt16(nameof(Max), attr.Max);
        attr.Boost = s.UInt16(nameof(Boost), attr.Boost);
        if (hasBackup)
            attr.Backup = s.UInt16(nameof(Backup), attr.Backup);
        s.End();
        return attr;
    }

    public void Apply(NumericOperation operation, ushort amount)
    {
        Current = operation.Apply16(Current, amount, 0, Max);
    }

    public void ApplyToMax(NumericOperation operation, ushort amount)
    {
        Max = operation.Apply16(Max, amount);
        if (Current > Max)
            Current = Max;
    }
}

public class CharacterAttributes : ICharacterAttributes
{
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