namespace UAlbion.Formats.MapEvents;

public enum QueryType : byte
{
    Switch = 0x0,
    Unk1 = 0x1,
    Unk4 = 0x4,
    HasPartyMember = 0x5,
    HasItem = 0x6,
    UsedItem = 0x7,
    PreviousActionResult = 0x9,
    ScriptDebugMode = 0xA,
    UnkC = 0xC,
    NpcActive = 0xE,
    Gold = 0xF,
    RandomChance = 0x11,
    Hour = 0x12,
    ChosenVerb = 0x14,
    Conscious = 0x15,
    Leader = 0x1A,
    Ticker = 0x1C,
    Map = 0x1D,
    Unk1E = 0x1E,
    PromptPlayer = 0x1F,
    Unk19 = 0x19,
    TriggerType = 0x20,
    Unk21 = 0x21,
    EventUsed = 0x22,
    DemoVersion = 0x23,
    Unk29 = 0x29,
    Unk2A = 0x2A,
    PromptPlayerNumeric = 0x2B
}