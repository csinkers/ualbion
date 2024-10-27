using System;
using UAlbion.Formats.Assets.Sheets;

namespace UAlbion.Editor;

public class CharacterSkillsEditor : AssetEditor
{
    readonly CharacterSkills _skills;

    public CharacterSkillsEditor(CharacterSkills skills) : base(skills)
    {
        _skills = skills ?? throw new ArgumentNullException(nameof(skills));
    }

    public override void Render()
    {
        UInt16Slider(nameof( _skills.CloseCombat),       _skills.CloseCombat.Current,    0, _skills.CloseCombat.Max);
        UInt16Slider(nameof( _skills.RangedCombat),      _skills.RangedCombat.Current,   0, _skills.RangedCombat.Max);
        UInt16Slider(nameof( _skills.CriticalChance),    _skills.CriticalChance.Current, 0, _skills.CriticalChance.Max);
        UInt16Slider(nameof( _skills.LockPicking),       _skills.LockPicking.Current,    0, _skills.LockPicking.Max);

        UInt16Slider(nameof( _skills.CloseCombat.Max),    _skills.CloseCombat.Max,    0, 100);
        UInt16Slider(nameof( _skills.RangedCombat.Max),   _skills.RangedCombat.Max,   0, 100);
        UInt16Slider(nameof( _skills.CriticalChance.Max), _skills.CriticalChance.Max, 0, 100);
        UInt16Slider(nameof( _skills.LockPicking.Max),    _skills.LockPicking.Max,    0, 100);
    }
}