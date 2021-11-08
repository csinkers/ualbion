namespace UAlbion.Formats.MapEvents
{
    public enum ActionType : byte
    {
        Word           = 0x0,
        AskAboutItem   = 0x1,
        Unk2           = 0x2, // Pay money? See ES156 (Garris, Gratogel sailor)
        Unk4           = 0x4,
        AskToLeave     = 0x5,
        StartDialogue  = 0x6,
        FinishDialogue = 0x7,
        DialogueLine   = 0x8,
        Unk9           = 0x9,
        UnkE           = 0xE,
        Unk17          = 0x17,
        Unk2D          = 0x2D,
        UseItem        = 0x2E,
        EquipItem      = 0x2F,
        UnequipItem    = 0x30,
        PickupItem     = 0x36,
        Unk39          = 0x39,
        Unk3D          = 0x3D
    }
}
