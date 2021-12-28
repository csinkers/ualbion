namespace UAlbion.Core.Visual;

public interface ISpriteManager : IRenderableSource
{
    SpriteLease Borrow(SpriteKey key, int count, object owner);
}