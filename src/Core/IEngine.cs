using UAlbion.Core.Visual;

namespace UAlbion.Core
{
    public interface IEngine : IComponent
    {
        void Run();
        void ChangeBackend();
        ICoreFactory Factory { get; }
        string FrameTimeText { get; }
        void RegisterRenderable(IRenderable renderable);
        void UnregisterRenderable(IRenderable renderable);
    }
}
