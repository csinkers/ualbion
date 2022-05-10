using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;

namespace UAlbion.Game;

public class DummyPaletteManager : IPaletteManager
{
    public DummyPaletteManager(AlbionPalette dayPalette, AlbionPalette nightPalette)
    {
        Day = dayPalette;
        Night = nightPalette;
    }

    public IPalette Day { get; }
    public IPalette Night { get; }
    public int Frame => 0;
    public float Blend => 0;
}