namespace UAlbion.Core
{
    public interface IScene : IContainer
    {
        ICamera Camera { get; }
    }
}
