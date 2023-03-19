using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

public class DummyPaletteManager : IPaletteManager
{
    public DummyPaletteManager(IPalette dayPalette, IPalette nightPalette)
    {
        Day = dayPalette;
        Night = nightPalette;
    }

    public IPalette Day { get; }
    public IPalette Night { get; }
    public int Frame => 0;
    public float Blend => 0;
}