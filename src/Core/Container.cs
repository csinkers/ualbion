using System;

namespace UAlbion.Core
{
    public class Container : Component, IContainer
    {
        readonly object _syncRoot = new();
        public string Name { get; }
        public Container(string name) { Name = name; }
        public Container(string name, params IComponent[] components)
        {
            Name = name;
            foreach (var component in components)
                Add(component);
        }

        public IContainer Add(IComponent child)
        {
            lock (_syncRoot)
                AttachChild(child);
            return this;
        }

        public void Remove(IComponent child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            lock (_syncRoot)
                child.Remove();
        }

        public override string ToString() => Name;
    }
}
