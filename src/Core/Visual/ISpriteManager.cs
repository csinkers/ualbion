using System.Collections.Generic;

namespace UAlbion.Core.Visual
{
    public interface ISpriteManager
    {
        IReadOnlyList<SpriteBatch> Batches { get; }
        SpriteLease Borrow(SpriteKey key, int count, object owner);
    }
}