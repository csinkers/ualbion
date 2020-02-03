using System.Collections.Generic;

namespace UAlbion.Api
{
    public interface IPalette
    {
        int Id { get; }
        string Name { get; }
        IList<uint[]> GetCompletePalette();
        bool IsAnimated { get; }
        uint[] GetPaletteAtTime(int paletteFrame);
    }
}