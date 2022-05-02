using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

public interface IPaletteManager
{
    IPalette Day { get; }
    IPalette Night { get; }
    int Version { get; }
    int Frame { get; }
    float Blend { get; }
}