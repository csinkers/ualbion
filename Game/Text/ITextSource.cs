using System.Collections.Generic;

namespace UAlbion.Game.Text
{
    public interface ITextSource
    {
        int Version { get; }
        IEnumerable<TextBlock> Get();
    }
}
