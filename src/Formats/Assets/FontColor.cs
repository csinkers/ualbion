using System;

namespace UAlbion.Formats.Assets;

public enum FontColor : byte
{
    White = 1,
    Yellow = 2,
    YellowOrange = 6
    /* Inks:
    1: Regular white
    2: Yellow
    6: Yellow/orange
    +64: Damaged
    ??: Gray
    */,
    Gray
}

public static class FontColorExtensions
{
    public static CommonColor GetLineColor(this FontColor color)
    {
        return color switch
        {
            FontColor.White => CommonColor.White,
            FontColor.Yellow => CommonColor.Yellow5,
            FontColor.YellowOrange => CommonColor.Orange5, // TODO: Verify
            FontColor.Gray => CommonColor.Grey6, // TODO: Verify
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };
    }
}