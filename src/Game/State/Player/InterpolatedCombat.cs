using System;
using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player;

public class InterpolatedCombat : ICombatAttributes
{
    readonly Func<ICombatAttributes> _a;
    readonly Func<ICombatAttributes> _b;
    readonly Func<float> _getLerp;

    public InterpolatedCombat(Func<ICombatAttributes> a, Func<ICombatAttributes> b, Func<float> getLerp)
    {
        _a = a;
        _b = b;
        _getLerp = getLerp;
        LifePoints = new InterpolatedAttribute(() => a().LifePoints, () => b().LifePoints, getLerp);
    }

    public int ExperiencePoints => (int)ApiUtil.Lerp(_a().ExperiencePoints, _b().ExperiencePoints, _getLerp());
    public ushort TrainingPoints => (ushort)ApiUtil.Lerp(_a().TrainingPoints, _b().TrainingPoints, _getLerp());
    public ICharacterAttribute LifePoints { get; }
    public byte ActionPoints => (byte)ApiUtil.Lerp(_a().ActionPoints, _b().ActionPoints, _getLerp());
    public ushort BaseDefense => (ushort)ApiUtil.Lerp(_a().BaseDefense, _b().BaseDefense, _getLerp());
    public short BonusDefense => (short)ApiUtil.Lerp(_a().BonusDefense, _b().BonusDefense, _getLerp());
    public ushort BaseAttack => (ushort)ApiUtil.Lerp(_a().BaseAttack, _b().BaseAttack, _getLerp());
    public short BonusAttack => (short)ApiUtil.Lerp(_a().BonusAttack, _b().BonusAttack, _getLerp());
    public PlayerConditions Conditions => _b().Conditions;
}