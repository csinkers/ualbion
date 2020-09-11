namespace UAlbion.Formats.MapEvents
{
    public enum PlaceActionType : byte
    {
        LearnCloseCombat = 0,
        Heal = 1,
        Cure = 2,
        RemoveCurse = 3,
        AskOpinion = 4,
        RestoreItemEnergy = 5,
        SleepInRoom = 6,
        Merchant = 7,
        OrderFood = 8,
        ScrollMerchant = 9,
        LearnSpells = 11, // Unk5 = Spell class
        RepairItem = 12,
    }
}
