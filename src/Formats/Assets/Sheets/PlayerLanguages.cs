using System;

namespace UAlbion.Formats.Assets.Sheets;

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

public static class PlayerLanguageExtensions
{
    public static PlayerLanguages ToFlag(this PlayerLanguage language) => language switch
    {
        PlayerLanguage.Terran => PlayerLanguages.Terran,
        PlayerLanguage.Iskai => PlayerLanguages.Iskai,
        PlayerLanguage.Celtic => PlayerLanguages.Celtic,
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
    };
}