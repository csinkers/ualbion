namespace UAlbion.Core
{
    public interface IContainer
    {
        IContainer Add<T>(T child) where T : IComponent;
        void Remove(IComponent child);
    }
}