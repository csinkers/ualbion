using System;
using UAlbion.Formats.Assets.Sheets;

namespace UAlbion.Game.State.Player;

public class InterpolatedSkills : ICharacterSkills
{
    public InterpolatedSkills(Func<ICharacterSkills> a, Func<ICharacterSkills> b, Func<float> getLerp)
    {
        CloseCombat = new InterpolatedAttribute(() => a().CloseCombat, () => b().CloseCombat, getLerp);
        RangedCombat = new InterpolatedAttribute(() => a().RangedCombat, () => b().RangedCombat, getLerp);
        CriticalChance = new InterpolatedAttribute(() => a().CriticalChance, () => b().CriticalChance, getLerp);
        LockPicking = new InterpolatedAttribute(() => a().LockPicking, () => b().LockPicking, getLerp);
    }

    public ICharacterAttribute CloseCombat { get; }
    public ICharacterAttribute RangedCombat { get; }
    public ICharacterAttribute CriticalChance { get; }
    public ICharacterAttribute LockPicking { get; }
}