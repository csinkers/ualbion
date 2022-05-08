namespace UAlbion.Core.Visual;

public interface ISpriteManager<TInstance> : IRenderableSource where TInstance : unmanaged
{
    SpriteLease<TInstance> Borrow(SpriteKey key, int count, object owner);
}