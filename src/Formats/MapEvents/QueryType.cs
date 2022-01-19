namespace UAlbion.Formats.MapEvents;

public enum QueryType : byte
{                                // Switch Offset
    Switch               = 0x0,  // 0x00
    ChainActive          = 0x1,  // 0x04 value = MapId (0=current map, id range: [1..512]), imm = chain number [0..250)
    DoorUnlocked         = 0x2,  // 0x08 value = DoorId
    ChestUnlocked        = 0x3,  // 0x0c value = ChestId
    NpcActiveOnMap       = 0x4,  // 0x10 value = MapId (0=current map), imm = npc number
    HasPartyMember       = 0x5,  // 0x14
    HasItem              = 0x6,  // 0x18
    UsedItem             = 0x7,  // 0x1c
    Unk8                 = 0x8,  // 0x20 SwitchType5
    PreviousActionResult = 0x9,  // 0x24
    ScriptDebugMode      = 0xA,  // 0x28
    UnkB                 = 0xB,  // 0x2c
    UnkC                 = 0xC,  // 0x30
    UnkD                 = 0xD,  // 0x34
    NpcActive            = 0xE,  // 0x38
    Gold                 = 0xF,  // 0x3c
    Rations              = 0x10, // 0x40
    RandomChance         = 0x11, // 0x44
    Hour                 = 0x12, // 0x48
    Unk13                = 0x13, // 0x4c
    ChosenVerb           = 0x14, // 0x50
    Conscious            = 0x15, // 0x54
    Gender               = 0x16, // 0x58
    Class                = 0x17, // 0x5c
    Race                 = 0x18, // 0x60
    Unk19                = 0x19, // 0x64
    Leader               = 0x1A, // 0x68
    Day                  = 0x1B, // 0x6c DayCount modulo immByte
    Ticker               = 0x1C, // 0x70
    Map                  = 0x1D, // 0x74
    Unk1E                = 0x1E, // 0x78 Clock tick related
    PromptPlayer         = 0x1F, // 0x7c
    TriggerType          = 0x20, // 0x80
    Unk21                = 0x21, // 0x84
    EventUsed            = 0x22, // 0x88
    DemoVersion          = 0x23, // 0x8c
    Unk24                = 0x24, // 0x90
    Unk25                = 0x25, // 0x94 Unused?
    Unk26                = 0x26, // 0x98
    Unk27                = 0x27, // 0x9c
    IsCurrentMap2D       = 0x28, // 0xa0 No params used, simple true/false
    NpcXCoord            = 0x29, // 0xa4 ImmByte: NpcNumber, or party if 0xff.
    NpcYCoord            = 0x2A, // 0xa8 ImmByte: NpcNumber, or party if 0xff.
    PromptPlayerNumeric  = 0x2B  // 0xac
}
