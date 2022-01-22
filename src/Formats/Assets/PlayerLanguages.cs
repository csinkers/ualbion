using System;

namespace UAlbion.Formats.Assets;

[Flags]
public enum PlayerLanguages : byte
{
    None = 0,
    Terran = 1,
    Iskai = 2,
    Celtic = 4
}

public enum PlayerLanguage : byte
{
    Terran = 0,
    Iskai = 1,
    Celtic = 2
}