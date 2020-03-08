namespace UAlbion.Core
{
    public interface IEngine
    {
        ICoreFactory Factory { get; }
        string FrameTimeText { get; }
    }
}