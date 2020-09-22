using System;
using UAlbion.Formats.Assets;

namespace UAlbion.Editor
{
    public class CombatAttributeEditor : AssetEditor
    {
        readonly CombatAttributes _combat;
        public CombatAttributeEditor(CombatAttributes combat) : base(combat)
        {
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
        }

        public override void Render()
        {
            Int32Slider(nameof(_combat.ExperiencePoints), _combat.ExperiencePoints, 0, ushort.MaxValue);
            UInt16Slider(nameof(_combat.TrainingPoints ), _combat.TrainingPoints, 0, ushort.MaxValue); 
            UInt16Slider(nameof(_combat.LifePoints     ), _combat.LifePoints    , 0, _combat.LifePointsMax); 
            UInt16Slider(nameof(_combat.LifePointsMax  ), _combat.LifePointsMax , 0, ushort.MaxValue); 
            UInt8Slider(nameof(_combat.ActionPoints   ), _combat.ActionPoints  , 0, byte.MaxValue); 
            UInt16Slider(nameof(_combat.Protection     ), _combat.Protection    , 0, 100); 
            UInt16Slider(nameof(_combat.Damage         ), _combat.Damage        , 0, 100);
            EnumCheckboxes(nameof(_combat.PhysicalConditions), _combat.PhysicalConditions);
            EnumCheckboxes(nameof(_combat.MentalConditions), _combat.MentalConditions);
        }
    }
}