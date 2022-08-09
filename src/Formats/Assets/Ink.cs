namespace UAlbion.Formats.Assets;

public class Ink
{
    public CommonColor PaletteLineColor { get; set; }
    public int[] PaletteMapping { get; set; }

    public int HueOffset { get; set; } // Hue offset angle, in range [0..360)
    public int ValueOffset { get; set; } // Value offset, in range [-100..100]
    public int SaturationOffset { get; set; } // Saturation offset, in range [-100..100]
}