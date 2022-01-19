using System;

namespace UAlbion.Formats.Assets;

[Flags]
public enum Conditions : ushort
{
    UnconsciousMask = Unconscious | Poisoned | Asleep,

    None        = 0,
    Unconscious = 1,       //   1 Unconscious
    Poisoned    = 1 <<  1, //   2 Poisoned
    Ill         = 1 <<  2, //   4 Ill
    Exhausted   = 1 <<  3, //   8 Exhausted
    Paralysed   = 1 <<  4, //  10 Paralysed
    Fleeing     = 1 <<  5, //  20 Fleeing
    Intoxicated = 1 <<  6, //  40 Intoxicated
    Blind       = 1 <<  7, //  80 Blind
    Panicking   = 1 <<  8, // 100 Panicking
    Asleep      = 1 <<  9, // 200 Asleep
    Insane      = 1 << 10, // 400 Insane
    Irritated   = 1 << 11, // 800 Irritated
    Unk12       = 1 << 12,
    Unk13       = 1 << 13,
    Unk14       = 1 << 14,
    Unk15       = 1 << 15,
}
