using System;
namespace UAlbion.Formats.MapEvents
{
    public enum ActionType : byte
    {
        Word = 0,
        AskAboutItem = 1,
        Unk2 = 2, // Pay money? See ES156 (Garris, Gratogel sailor)
        Unk4 = 4,
        Unk5 = 5,
        StartDialogue = 6,
        FinishDialogue = 7,
        DialogueLine = 8,
        Unk9 = 9,
        Unk14 = 14,
        Unk23 = 23,
        Unk45 = 45,
        UseItem = 46,
        EquipItem = 47,
        UnequipItem = 48,
        PickupItem = 54,
        Unk57 = 57,
        Unk61 = 61
    }
}
