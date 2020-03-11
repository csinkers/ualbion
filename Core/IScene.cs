namespace UAlbion.Core
{
    public interface IScene
    {
        void Add(IRenderable renderable);
        void Remove(IRenderable renderable);
        ICamera Camera { get; }
    }
}
