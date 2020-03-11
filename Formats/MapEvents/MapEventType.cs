namespace UAlbion.Formats.MapEvents
{
    public enum MapEventType : byte
    {
        Script = 0,
        MapExit = 1,
        Door = 2,
        Chest = 3,
        Text = 4,
        Spinner = 5,
        Trap = 6,
        ChangeUsedItem = 7,
        DataChange = 8,
        ChangeIcon = 9,
        Encounter = 0xA,
        PlaceAction = 0xB,
        Query = 0xC,
        Modify = 0xD,
        Action = 0xE,
        Signal = 0xF,
        CloneAutomap = 0x10,
        Sound = 0x11,
        StartDialogue = 0x12,
        CreateTransport = 0x13,
        Execute = 0x14,
        RemovePartyMember = 0x15,
        EndDialogue = 0x16,
        Wipe = 0x17,
        PlayAnimation = 0x18,
        Offset = 0x19,
        Pause = 0x1A,
        SimpleChest = 0x1B,
        AskSurrender = 0x1C,
        DoScript = 0x1D,
        UnkFF = 0xFF // 3D only?
    }
}
