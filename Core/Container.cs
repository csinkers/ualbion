using System;
using System.Collections.Generic;

namespace UAlbion.Core
{
    public class Container : Component, IContainer
    {
        public Container() { }
        protected Container(IDictionary<Type, Handler> handlers) : base(handlers) { }
        public IContainer Add<T>(T child) where T : IComponent
        {
            AttachChild(child);
            return this;
        }

        public void Remove(IComponent child)
        {
            if (Children.Remove(child))
                child.Detach();
        }
    }
}