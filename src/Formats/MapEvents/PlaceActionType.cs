namespace UAlbion.Formats.MapEvents
{
    public enum PlaceActionType : byte
    {
        LearnCloseCombat  = 0x0,
        Heal              = 0x1,
        Cure              = 0x2,
        RemoveCurse       = 0x3,
        AskOpinion        = 0x4,
        RestoreItemEnergy = 0x5,
        SleepInRoom       = 0x6,
        Merchant          = 0x7,
        OrderFood         = 0x8,
        ScrollMerchant    = 0x9,
        LearnSpells       = 0xB, // Unk5 = Spell class
        RepairItem        = 0xC,
    }
}
