namespace UAlbion.Core.Visual
{
    public interface ISpriteManager
    {
        SpriteLease Borrow(SpriteKey key, int length, object caller);
        void Cleanup();
    }
}