using System.Collections.Generic;

namespace UAlbion.Core.Visual
{
    public interface ISpriteManager
    {
        IReadOnlyList<SpriteBatch> Ordered { get; }
        SpriteLease Borrow(SpriteKey key, int count, object owner);
    }
}