using System;

namespace UAlbion.Formats.Assets;

public enum PlayerCondition : ushort
{
    Unconscious = 0,
    Poisoned = 1,
    Ill = 2,
    Exhausted = 3,
    Paralysed = 4,
    Fleeing = 5,
    Intoxicated = 6,
    Blind = 7,
    Panicking = 8,
    Asleep = 9,
    Insane = 10,
    Irritated = 11
}

public static class PlayerConditionExtensions
{
    public static PlayerConditions ToFlag(this PlayerCondition condition) => condition switch
    {
        PlayerCondition.Unconscious => PlayerConditions.Unconscious,
        PlayerCondition.Poisoned    => PlayerConditions.Poisoned,
        PlayerCondition.Ill         => PlayerConditions.Ill,
        PlayerCondition.Exhausted   => PlayerConditions.Exhausted,
        PlayerCondition.Paralysed   => PlayerConditions.Paralysed,
        PlayerCondition.Fleeing     => PlayerConditions.Fleeing,
        PlayerCondition.Intoxicated => PlayerConditions.Intoxicated,
        PlayerCondition.Blind       => PlayerConditions.Blind,
        PlayerCondition.Panicking   => PlayerConditions.Panicking,
        PlayerCondition.Asleep      => PlayerConditions.Asleep,
        PlayerCondition.Insane      => PlayerConditions.Insane,
        PlayerCondition.Irritated   => PlayerConditions.Irritated,
        _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, null)
    };
}
