using System;
using UAlbion.Core;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player
{
    public class InterpolatedAttributes : ICharacterAttributes
    {
        readonly Func<ICharacterAttributes> _a;
        readonly Func<ICharacterAttributes> _b;
        readonly Func<float> _getLerp;

        public InterpolatedAttributes(Func<ICharacterAttributes> a, Func<ICharacterAttributes> b, Func<float> getLerp)
        {
            _a = a;
            _b = b;
            _getLerp = getLerp;
        }

        public ushort Strength => (ushort)Util.Lerp(_a().Strength, _b().Strength, _getLerp());
        public ushort Intelligence => (ushort)Util.Lerp(_a().Intelligence, _b().Intelligence, _getLerp());
        public ushort Dexterity => (ushort)Util.Lerp(_a().Dexterity, _b().Dexterity, _getLerp());
        public ushort Speed => (ushort)Util.Lerp(_a().Speed, _b().Speed, _getLerp());
        public ushort Stamina => (ushort)Util.Lerp(_a().Stamina, _b().Stamina, _getLerp());
        public ushort Luck => (ushort)Util.Lerp(_a().Luck, _b().Luck, _getLerp());
        public ushort MagicResistance => (ushort)Util.Lerp(_a().MagicResistance, _b().MagicResistance, _getLerp());
        public ushort MagicTalent => (ushort)Util.Lerp(_a().MagicTalent, _b().MagicTalent, _getLerp());
        public ushort StrengthMax => (ushort)Util.Lerp(_a().StrengthMax, _b().StrengthMax, _getLerp());
        public ushort IntelligenceMax => (ushort)Util.Lerp(_a().IntelligenceMax, _b().IntelligenceMax, _getLerp());
        public ushort DexterityMax => (ushort)Util.Lerp(_a().DexterityMax, _b().DexterityMax, _getLerp());
        public ushort SpeedMax => (ushort)Util.Lerp(_a().SpeedMax, _b().SpeedMax, _getLerp());
        public ushort StaminaMax => (ushort)Util.Lerp(_a().StaminaMax, _b().StaminaMax, _getLerp());
        public ushort LuckMax => (ushort)Util.Lerp(_a().LuckMax, _b().LuckMax, _getLerp());
        public ushort MagicResistanceMax => (ushort)Util.Lerp(_a().MagicResistanceMax, _b().MagicResistanceMax, _getLerp());
        public ushort MagicTalentMax => (ushort)Util.Lerp(_a().MagicTalentMax, _b().MagicTalentMax, _getLerp());
    }
}