using UAlbion.Core.Visual;

namespace UAlbion.Core
{
    public abstract class Engine : ServiceComponent<IEngine>, IEngine
    {
        public abstract void Run();
        public abstract void ChangeBackend();
        public abstract ICoreFactory Factory { get; }
        public abstract string FrameTimeText { get; }
        public abstract bool IsDepthRangeZeroToOne { get; }
        public abstract bool IsClipSpaceYInverted { get; }
        public abstract void RegisterRenderable(IRenderable renderable);
        public abstract void UnregisterRenderable(IRenderable renderable);
        
        /// <summary>
        /// The global event exchange.
        /// This should only be used rarely, by non-component objects.
        /// Components should use their own Exchange instead.
        /// </summary>
        public static EventExchange GlobalExchange { get; set; } 
    }
}
