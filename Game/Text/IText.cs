using System.Collections.Generic;

namespace UAlbion.Game.Text
{
    public interface IText
    {
        int Version { get; }
        IEnumerable<TextBlock> Get();
    }
}
