using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public abstract class Engine : Component, IEngine
    {
        public abstract ICoreFactory Factory { get; }
        public abstract string FrameTimeText { get; }
        public static EventExchange GlobalExchange { get; set; }
    }
}
