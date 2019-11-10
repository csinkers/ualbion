using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Entities
{
    public interface ITextManager
    {
        Vector2 Measure(TextBlock block);
        IPositionedRenderable BuildRenderable(TextBlock block, out Vector2 size);
        IEnumerable<TextBlock> SplitBlocksToSingleWords(IEnumerable<TextBlock> blocks);
    }
}