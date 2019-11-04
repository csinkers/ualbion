using System.Collections.Generic;

namespace UAlbion.Game
{
    public interface ITextSource
    {
        int Version { get; }
        IEnumerable<TextBlock> Get();
    }
}