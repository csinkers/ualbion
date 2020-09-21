using System;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Veldrid.Editor
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
            IntSlider(nameof( _skills.CloseCombat),       _skills.CloseCombat,       0, _skills.CloseCombatMax);
            IntSlider(nameof( _skills.RangedCombat),      _skills.RangedCombat,      0, _skills.RangedCombatMax);
            IntSlider(nameof( _skills.CriticalChance),    _skills.CriticalChance,    0, _skills.CriticalChanceMax);
            IntSlider(nameof( _skills.LockPicking),       _skills.LockPicking,       0, _skills.LockPickingMax);

            IntSlider(nameof( _skills.CloseCombatMax),    _skills.CloseCombatMax,    0, 100);
            IntSlider(nameof( _skills.RangedCombatMax),   _skills.RangedCombatMax,   0, 100);
            IntSlider(nameof( _skills.CriticalChanceMax), _skills.CriticalChanceMax, 0, 100);
            IntSlider(nameof( _skills.LockPickingMax),    _skills.LockPickingMax,    0, 100);
        }
    }
}