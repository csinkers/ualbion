using System.Collections.Generic;

namespace UAlbion.Api.Visual;

public interface IPalette
{
    uint Id { get; }
    string Name { get; }
    IList<uint[]> GetCompletePalette();
    bool IsAnimated { get; }
    uint[] GetPaletteAtTime(int paletteFrame);
    IEnumerable<(byte, int)> AnimatedEntries { get; }
}