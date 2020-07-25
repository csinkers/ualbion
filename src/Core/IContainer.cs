namespace UAlbion.Core
{
    public interface IContainer
    {
        IContainer Add(IComponent child);
        void Remove(IComponent child);
    }
}