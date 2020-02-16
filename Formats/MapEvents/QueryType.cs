namespace UAlbion.Formats.MapEvents
{
    public enum QueryType : byte
    {
        TemporarySwitch = 0x0,
        HasPartyMember = 0x5,
        InventoryHasItem = 0x6,
        UsedItemId = 0x7,
        PreviousActionResult = 0x9,
        IsScriptDebugModeActive = 0xA,
        IsNpcActive = 0xE,
        HasEnoughGold = 0xF,
        RandomChance = 0x11,
        ChosenVerb = 0x14,
        IsPartyMemberConscious = 0x15,
        IsPartyMemberLeader = 0x1A,
        Ticker = 0x1C,
        CurrentMapId = 0x1D,
        PromptPlayer = 0x1F,
        TriggerType = 0x20,
        EventAlreadyUsed = 0x22,
        IsDemoVersion = 0x23,
        PromptPlayerNumeric = 0x2B
    }
}