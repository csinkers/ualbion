using System;
using UAlbion.Formats.Assets;

namespace UAlbion.Editor
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
            UInt16Slider(nameof(_attributes.Strength),        _attributes.Strength,        0, _attributes.StrengthMax);
            UInt16Slider(nameof(_attributes.Intelligence),    _attributes.Intelligence,    0, _attributes.IntelligenceMax);
            UInt16Slider(nameof(_attributes.Dexterity),       _attributes.Dexterity,       0, _attributes.DexterityMax);
            UInt16Slider(nameof(_attributes.Speed),           _attributes.Speed,           0, _attributes.SpeedMax);
            UInt16Slider(nameof(_attributes.Stamina),         _attributes.Stamina,         0, _attributes.StaminaMax);
            UInt16Slider(nameof(_attributes.Luck),            _attributes.Luck,            0, _attributes.LuckMax);
            UInt16Slider(nameof(_attributes.MagicResistance), _attributes.MagicResistance, 0, _attributes.MagicResistanceMax);
            UInt16Slider(nameof(_attributes.MagicTalent),     _attributes.MagicTalent,     0, _attributes.MagicTalentMax);

            UInt16Slider(nameof(_attributes.StrengthMax),        _attributes.StrengthMax,        0, 100);
            UInt16Slider(nameof(_attributes.IntelligenceMax),    _attributes.IntelligenceMax,    0, 100);
            UInt16Slider(nameof(_attributes.DexterityMax),       _attributes.DexterityMax,       0, 100);
            UInt16Slider(nameof(_attributes.SpeedMax),           _attributes.SpeedMax,           0, 100);
            UInt16Slider(nameof(_attributes.StaminaMax),         _attributes.StaminaMax,         0, 100);
            UInt16Slider(nameof(_attributes.LuckMax),            _attributes.LuckMax,            0, 100);
            UInt16Slider(nameof(_attributes.MagicResistanceMax), _attributes.MagicResistanceMax, 0, 100);
            UInt16Slider(nameof(_attributes.MagicTalentMax),     _attributes.MagicTalentMax,     0, 100);
        }
    }
}