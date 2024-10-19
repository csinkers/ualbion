using System;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.Assets;

public class CombatAttributes : ICombatAttributes
{
    [DiagEdit(Style = DiagEditStyle.NumericInput, Min = 0)]
    public int ExperiencePoints { get; set; }

    [DiagEdit(Style = DiagEditStyle.NumericInput, Min = 0)]
    public ushort TrainingPoints { get; set; }

    ICharacterAttribute ICombatAttributes.LifePoints => LifePoints;

    [DiagEdit(Style = DiagEditStyle.CharacterAttribute)]
    public CharacterAttribute LifePoints { get; set; }

    [DiagEdit(Style = DiagEditStyle.NumericInput, Min = 0)]
    public byte ActionPoints { get; set; }
    public ushort BaseDefense { get; set; } // Intrinsic defense (monsters only?)
    public short BonusDefense { get; set; } // Due to equipment
    public ushort BaseAttack { get; set; } // Intrinsic damage (monsters only?)
    public short BonusAttack { get; set; } // Due to equipment
    public ushort MagicAttack { get; set; }
    public ushort MagicDefense { get; set; }

    [DiagEdit(Style = DiagEditStyle.Checkboxes)]
    public PlayerConditions Conditions { get; set; }
    public CombatAttributes DeepClone() => new CombatAttributes().CopyFrom(this);
    public CombatAttributes CopyFrom(CombatAttributes other)
    {
        ArgumentNullException.ThrowIfNull(other);

        ExperiencePoints = other.ExperiencePoints;
        TrainingPoints = other.TrainingPoints;
        LifePoints = other.LifePoints.DeepClone();
        ActionPoints = other.ActionPoints;
        BaseDefense = other.BaseDefense;
        BonusDefense = other.BonusDefense;
        Conditions = other.Conditions;
        return this;
    }

    public override string ToString() => $"XP:{ExperiencePoints} TP:{TrainingPoints} LP:{LifePoints} AP:{ActionPoints} D:{BonusDefense} P:{BaseDefense} Cond:{Conditions}";
}