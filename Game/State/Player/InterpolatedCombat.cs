using System;
using UAlbion.Core;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player
{
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
        }

        public uint ExperiencePoints => (ushort)Util.Lerp(_a().ExperiencePoints, _b().ExperiencePoints, _getLerp());
        public ushort TrainingPoints => (ushort)Util.Lerp(_a().TrainingPoints, _b().TrainingPoints, _getLerp());
        public ushort LifePoints => (ushort)Util.Lerp(_a().LifePoints, _b().LifePoints, _getLerp());
        public ushort LifePointsMax => (ushort)Util.Lerp(_a().LifePointsMax, _b().LifePointsMax, _getLerp());
        public byte ActionPoints => (byte)Util.Lerp(_a().ActionPoints, _b().ActionPoints, _getLerp());
        public ushort Protection => (ushort)Util.Lerp(_a().Protection, _b().Protection, _getLerp());
        public ushort Damage => (ushort)Util.Lerp(_a().Damage, _b().Damage, _getLerp());
        public PhysicalCondition PhysicalConditions => _b().PhysicalConditions;
        public MentalCondition MentalConditions => _b().MentalConditions;
    }
}