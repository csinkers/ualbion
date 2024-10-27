using System;
using UAlbion.Formats.Assets.Sheets;

namespace UAlbion.Editor;

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
        UInt16Slider(nameof(_combat.TrainingPoints), _combat.TrainingPoints, 0, ushort.MaxValue);
        UInt16Slider(nameof(_combat.LifePoints), _combat.LifePoints.Current, 0, _combat.LifePoints.Max);
        UInt16Slider(nameof(_combat.LifePoints.Max), _combat.LifePoints.Max, 0, ushort.MaxValue);
        UInt8Slider(nameof(_combat.ActionPoints), _combat.ActionPoints, 0, byte.MaxValue);
        UInt16Slider(nameof(_combat.BaseDefense), _combat.BaseDefense, 0, 100);
        Int16Slider(nameof(_combat.BonusDefense), _combat.BonusDefense, 0, 100);
        EnumCheckboxes(nameof(_combat.Conditions), _combat.Conditions);
    }
}