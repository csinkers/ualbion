using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;

namespace UAlbion.Game.Entities
{
    public interface ITextManager
    {
        Vector2 Measure(TextBlock block);
        PositionedSpriteBatch BuildRenderable(TextBlock block, DrawLayer layer, object caller);
        IEnumerable<TextBlock> SplitBlocksToSingleWords(IEnumerable<TextBlock> blocks);
    }
}