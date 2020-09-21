using System;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Veldrid.Editor
{
    public class CharacterAttributesEditor : AssetEditor
    {
        readonly CharacterAttributes _attributes;

        public CharacterAttributesEditor(CharacterAttributes attributes) : base(attributes)
        {
            _attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        }

        public override void Render()
        {
            IntSlider(nameof(_attributes.Strength),        _attributes.Strength,        0, _attributes.StrengthMax);
            IntSlider(nameof(_attributes.Intelligence),    _attributes.Intelligence,    0, _attributes.IntelligenceMax);
            IntSlider(nameof(_attributes.Dexterity),       _attributes.Dexterity,       0, _attributes.DexterityMax);
            IntSlider(nameof(_attributes.Speed),           _attributes.Speed,           0, _attributes.SpeedMax);
            IntSlider(nameof(_attributes.Stamina),         _attributes.Stamina,         0, _attributes.StaminaMax);
            IntSlider(nameof(_attributes.Luck),            _attributes.Luck,            0, _attributes.LuckMax);
            IntSlider(nameof(_attributes.MagicResistance), _attributes.MagicResistance, 0, _attributes.MagicResistanceMax);
            IntSlider(nameof(_attributes.MagicTalent),     _attributes.MagicTalent,     0, _attributes.MagicTalentMax);

            IntSlider(nameof(_attributes.StrengthMax),        _attributes.StrengthMax,        0, ushort.MaxValue);
            IntSlider(nameof(_attributes.IntelligenceMax),    _attributes.IntelligenceMax,    0, ushort.MaxValue);
            IntSlider(nameof(_attributes.DexterityMax),       _attributes.DexterityMax,       0, ushort.MaxValue);
            IntSlider(nameof(_attributes.SpeedMax),           _attributes.SpeedMax,           0, ushort.MaxValue);
            IntSlider(nameof(_attributes.StaminaMax),         _attributes.StaminaMax,         0, ushort.MaxValue);
            IntSlider(nameof(_attributes.LuckMax),            _attributes.LuckMax,            0, ushort.MaxValue);
            IntSlider(nameof(_attributes.MagicResistanceMax), _attributes.MagicResistanceMax, 0, ushort.MaxValue);
            IntSlider(nameof(_attributes.MagicTalentMax),     _attributes.MagicTalentMax,     0, ushort.MaxValue);
        }
    }
}