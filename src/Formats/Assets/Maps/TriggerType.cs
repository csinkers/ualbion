using System;

namespace UAlbion.Formats.Assets.Maps;

public enum TriggerType : ushort
{
    Normal = 0,
    Examine = 1,
    Manipulate = 2,
    TalkTo = 3,
    UseItem = 4,
    MapInit = 5,
    EveryStep = 6,
    EveryHour = 7,
    EveryDay = 8,
    Default = 9,
    Action = 10,
    Npc = 11,
    Take = 12,
    Unk13 = 13,
    Unk14 = 14,
    Unk15 = 15,
}

public static class TriggerTypeExtensions
{
    public static TriggerTypes ToBitField(this TriggerType type)
    {
        return type switch {
            TriggerType.Normal => TriggerTypes.Normal,
            TriggerType.Examine => TriggerTypes.Examine,
            TriggerType.Manipulate => TriggerTypes.Manipulate,
            TriggerType.TalkTo => TriggerTypes.TalkTo,
            TriggerType.UseItem => TriggerTypes.UseItem,
            TriggerType.MapInit => TriggerTypes.MapInit,
            TriggerType.EveryStep => TriggerTypes.EveryStep,
            TriggerType.EveryHour => TriggerTypes.EveryHour,
            TriggerType.EveryDay => TriggerTypes.EveryDay,
            TriggerType.Default => TriggerTypes.Default,
            TriggerType.Action => TriggerTypes.Action,
            TriggerType.Npc => TriggerTypes.Npc,
            TriggerType.Take => TriggerTypes.Take,
            TriggerType.Unk13 => TriggerTypes.Unk13,
            TriggerType.Unk14 => TriggerTypes.Unk14,
            TriggerType.Unk15 => TriggerTypes.Unk15,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
