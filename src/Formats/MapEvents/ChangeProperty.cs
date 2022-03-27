namespace UAlbion.Formats.MapEvents;

public enum ChangeProperty : byte
{
    Attribute   = 0x0,
    Skill       = 0x1,
    Health      = 0x2,
    Mana        = 0x3,
    Unused4     = 0x4,
    Status      = 0x5,
    Unused6     = 0x6,
    Language    = 0x7,
    Experience  = 0x8,
    TrainingPoints = 0x9,
    UnusedA     = 0xA,
    UnusedB     = 0xB,
    EventSetId  = 0xC,
    WordSetId   = 0xD,
    UnusedE     = 0xE,
    UnusedF     = 0xF,
    KnownSpells = 0x10,
    MaxHealth   = 0x11,
    MaxMana     = 0x12,
    Item        = 0x13,
    Gold        = 0x14,
    Food        = 0x15
}
