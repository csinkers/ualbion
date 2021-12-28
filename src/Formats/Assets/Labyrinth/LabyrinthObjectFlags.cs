﻿using System;

namespace UAlbion.Formats.Assets.Labyrinth;

[Flags]
public enum LabyrinthObjectFlags : byte
{
    Unk0 = 1,
    Unk1 = 1 << 1,
    FloorObject = 1 << 2,
    Unk3 = 1 << 3,
    Unk4 = 1 << 4,
    Unk5 = 1 << 5,
    Unk6 = 1 << 6,
    Unk7 = 1 << 7,
}