namespace UAlbion.Api.Eventing;

public interface IContainer : IComponent
{
    IContainer Add(IComponent child);
    void Remove(IComponent child);
}