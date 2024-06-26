﻿using System;
namespace UAlbion.Formats.Assets;

public class CombatAttributes : ICombatAttributes
{
    public int ExperiencePoints { get; set; }
    public ushort TrainingPoints { get; set; }
    ICharacterAttribute ICombatAttributes.LifePoints => LifePoints;
    public CharacterAttribute LifePoints { get; set; }
    public byte ActionPoints { get; set; }
    public ushort UnknownD6 { get; set; }
    public ushort UnknownD8 { get; set; }
    public PlayerConditions Conditions { get; set; }
    public CombatAttributes DeepClone() => new CombatAttributes().CopyFrom(this);
    public CombatAttributes CopyFrom(CombatAttributes other)
    {
        ArgumentNullException.ThrowIfNull(other);

        ExperiencePoints = other.ExperiencePoints;
        TrainingPoints = other.TrainingPoints;
        LifePoints = other.LifePoints.DeepClone();
        ActionPoints = other.ActionPoints;
        UnknownD6 = other.UnknownD6;
        UnknownD8 = other.UnknownD8;
        Conditions = other.Conditions;
        return this;
    }

    public override string ToString() => $"XP:{ExperiencePoints} TP:{TrainingPoints} LP:{LifePoints} AP:{ActionPoints} D:{UnknownD8} P:{UnknownD6} Cond:{Conditions}";
}