namespace UAlbion.Api.Eventing;

public interface IContainer
{
    IContainer Add(IComponent child);
    void Remove(IComponent child);
}