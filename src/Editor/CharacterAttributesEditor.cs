using System;
using UAlbion.Formats.Assets;

namespace UAlbion.Editor;

public class CharacterAttributesEditor : AssetEditor
{
    readonly CharacterAttributes _attributes;

    public CharacterAttributesEditor(CharacterAttributes attributes) : base(attributes)
    {
        _attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
    }

    public override void Render()
    {
        UInt16Slider(nameof(_attributes.Strength),        _attributes.Strength.Current,        0, _attributes.Strength.Max);
        UInt16Slider(nameof(_attributes.Intelligence),    _attributes.Intelligence.Current,    0, _attributes.Intelligence.Max);
        UInt16Slider(nameof(_attributes.Dexterity),       _attributes.Dexterity.Current,       0, _attributes.Dexterity.Max);
        UInt16Slider(nameof(_attributes.Speed),           _attributes.Speed.Current,           0, _attributes.Speed.Max);
        UInt16Slider(nameof(_attributes.Stamina),         _attributes.Stamina.Current,         0, _attributes.Stamina.Max);
        UInt16Slider(nameof(_attributes.Luck),            _attributes.Luck.Current,            0, _attributes.Luck.Max);
        UInt16Slider(nameof(_attributes.MagicResistance), _attributes.MagicResistance.Current, 0, _attributes.MagicResistance.Max);
        UInt16Slider(nameof(_attributes.MagicTalent),     _attributes.MagicTalent.Current,     0, _attributes.MagicTalent.Max);

        UInt16Slider(nameof(_attributes.Strength.Max),        _attributes.Strength.Max,        0, 100);
        UInt16Slider(nameof(_attributes.Intelligence.Max),    _attributes.Intelligence.Max,    0, 100);
        UInt16Slider(nameof(_attributes.Dexterity.Max),       _attributes.Dexterity.Max,       0, 100);
        UInt16Slider(nameof(_attributes.Speed.Max),           _attributes.Speed.Max,           0, 100);
        UInt16Slider(nameof(_attributes.Stamina.Max),         _attributes.Stamina.Max,         0, 100);
        UInt16Slider(nameof(_attributes.Luck.Max),            _attributes.Luck.Max,            0, 100);
        UInt16Slider(nameof(_attributes.MagicResistance.Max), _attributes.MagicResistance.Max, 0, 100);
        UInt16Slider(nameof(_attributes.MagicTalent.Max),     _attributes.MagicTalent.Max,     0, 100);
    }
}