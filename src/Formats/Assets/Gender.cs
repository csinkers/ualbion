using System;

namespace UAlbion.Formats.Assets;

public enum Gender : byte
{
    Male = 0,
    Female = 1,
    Neuter = 2,
}

[Flags]
public enum Genders : byte
{
    Male = 1,
    Female = 2,
    Any = 3,
    Neutral = 4,
}