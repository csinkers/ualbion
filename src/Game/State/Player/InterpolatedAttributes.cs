using System;
using UAlbion.Formats.Assets.Sheets;

namespace UAlbion.Game.State.Player;

public class InterpolatedAttributes : ICharacterAttributes
{
    public InterpolatedAttributes(Func<ICharacterAttributes> a, Func<ICharacterAttributes> b, Func<float> getLerp)
    {
        Strength        = new InterpolatedAttribute(() => a().Strength,       () => b().Strength,        getLerp);
        Intelligence    = new InterpolatedAttribute(() => a().Intelligence,   () => b().Intelligence,    getLerp);
        Dexterity       = new InterpolatedAttribute(() => a().Dexterity,      () => b().Dexterity,       getLerp);
        Speed           = new InterpolatedAttribute(() => a().Speed,          () => b().Speed,           getLerp);
        Stamina         = new InterpolatedAttribute(() => a().Stamina,        () => b().Stamina,         getLerp);
        Luck            = new InterpolatedAttribute(() => a().Luck,           () => b().Luck,            getLerp);
        MagicResistance = new InterpolatedAttribute(() => a().MagicResistance,() => b().MagicResistance, getLerp);
        MagicTalent     = new InterpolatedAttribute(() => a().MagicTalent,    () => b().MagicTalent,     getLerp);
    }

    public ICharacterAttribute Strength { get; }
    public ICharacterAttribute Intelligence { get; }
    public ICharacterAttribute Dexterity { get; }
    public ICharacterAttribute Speed { get; }
    public ICharacterAttribute Stamina { get; }
    public ICharacterAttribute Luck { get; }
    public ICharacterAttribute MagicResistance { get; }
    public ICharacterAttribute MagicTalent { get; }
}
