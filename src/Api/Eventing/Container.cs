using System;

namespace UAlbion.Api.Eventing;

public class Container : Component, IContainer
{
    readonly object _syncRoot = new();
    public virtual string Name { get; }
    public Container(string name) { Name = name; }
    public Container(string name, params IComponent[] components)
    {
        if (components == null) throw new ArgumentNullException(nameof(components));
        Name = name;
        foreach (var component in components)
            Add(component);
    }

    protected virtual bool AddingChild(IComponent child) => true;
    protected virtual bool RemovingChild(IComponent child) => true;

    public IContainer Add(IComponent child)
    {
        if (!AddingChild(child))
            return this;

        lock (_syncRoot)
            AttachChild(child);
        return this;
    }

    public void Remove(IComponent child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        if (!RemovingChild(child))
            return;

        lock (_syncRoot)
            child.Remove();
    }

    public override string ToString() => Name;
}