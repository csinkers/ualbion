namespace UAlbion.Formats.MapEvents
{
    public enum QueryType : byte
    {
        TemporarySwitch = 0x0,
        Unk1 = 0x1,
        Unk4 = 0x4,
        HasPartyMember = 0x5,
        InventoryHasItem = 0x6,
        UsedItemId = 0x7,
        PreviousActionResult = 0x9,
        IsScriptDebugModeActive = 0xA,
        UnkC = 0xC,
        IsNpcActive = 0xE,
        HasEnoughGold = 0xF,
        RandomChance = 0x11,
        Unk12 = 0x12,
        ChosenVerb = 0x14,
        IsPartyMemberConscious = 0x15,
        IsPartyMemberLeader = 0x1A,
        Ticker = 0x1C,
        CurrentMapId = 0x1D,
        Unk1E = 0x1E,
        PromptPlayer = 0x1F,
        Unk19 = 0x19,
        Unk1A = 0x1A,
        TriggerType = 0x20,
        Unk21 = 0x21,
        EventAlreadyUsed = 0x22,
        IsDemoVersion = 0x23,
        PromptPlayerNumeric = 0x2B
    }
}
