using System;

namespace UAlbion.Formats.Assets;

[Flags]
public enum PlayerConditions : ushort
{
    UnconsciousMask = Unconscious | Poisoned | Asleep,

    None        =    0x0,
    Unconscious =    0x1, // Unconscious
    Poisoned    =    0x2, // Poisoned
    Ill         =    0x4, // Ill
    Exhausted   =    0x8, // Exhausted
    Paralysed   =   0x10, // Paralysed
    Fleeing     =   0x20, // Fleeing
    Intoxicated =   0x40, // Intoxicated
    Blind       =   0x80, // Blind
    Panicking   =  0x100, // Panicking
    Asleep      =  0x200, // Asleep
    Insane      =  0x400, // Insane
    Irritated   =  0x800, // Irritated
    Unk12       = 0x1000,
    Unk13       = 0x2000,
    Unk14       = 0x4000,
    Unk15       = 0x8000,
}
