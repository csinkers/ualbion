using System;
using UAlbion.Formats.Assets;

namespace UAlbion.Editor
{
    public class CharacterSkillsEditor : AssetEditor
    {
        readonly CharacterSkills _skills;

        public CharacterSkillsEditor(CharacterSkills Skills) : base(Skills)
        {
            _skills = Skills ?? throw new ArgumentNullException(nameof(Skills));
        }

        public override void Render()
        {
            UInt16Slider(nameof( _skills.CloseCombat),       _skills.CloseCombat,       0, _skills.CloseCombatMax);
            UInt16Slider(nameof( _skills.RangedCombat),      _skills.RangedCombat,      0, _skills.RangedCombatMax);
            UInt16Slider(nameof( _skills.CriticalChance),    _skills.CriticalChance,    0, _skills.CriticalChanceMax);
            UInt16Slider(nameof( _skills.LockPicking),       _skills.LockPicking,       0, _skills.LockPickingMax);

            UInt16Slider(nameof( _skills.CloseCombatMax),    _skills.CloseCombatMax,    0, 100);
            UInt16Slider(nameof( _skills.RangedCombatMax),   _skills.RangedCombatMax,   0, 100);
            UInt16Slider(nameof( _skills.CriticalChanceMax), _skills.CriticalChanceMax, 0, 100);
            UInt16Slider(nameof( _skills.LockPickingMax),    _skills.LockPickingMax,    0, 100);
        }
    }
}