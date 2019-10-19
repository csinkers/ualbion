using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public interface IScene
    {
        void Add(IRenderable renderable);
        void Remove(IRenderable renderable);
        EventExchange SceneExchange { get; }
    }
}