namespace UAlbion.Core
{
    public abstract class Engine : ServiceComponent<IEngine>, IEngine
    {
        public abstract void Run();
        public abstract void ChangeBackend();
        public abstract ICoreFactory Factory { get; }
        public abstract string FrameTimeText { get; }
        public static EventExchange GlobalExchange { get; set; }
    }
}
