using System;

namespace UAlbion.Formats.Assets
{
    [Flags]
    public enum TriggerType : ushort
    {
        Normal = 1 << 0,
        Examine = 1 << 1,
        Manipulate = 1 << 2,
        TalkTo = 1 << 3,
        UseItem = 1 << 4,
        MapInit = 1 << 5,
        EveryStep = 1 << 6,
        EveryHour = 1 << 7,
        EveryDay = 1 << 8,
        Default = 1 << 9,
        Action = 1 << 10,
        Npc = 1 << 11,
        Take = 1 << 12,
        Unk13 = 1 << 13,
        Unk14 = 1 << 14,
        Unk15 = 1 << 15,
    }
}