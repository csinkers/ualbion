using System;
using System.Collections.Generic;

namespace UAlbion.Core
{
    public abstract class ServiceComponent<T> : Component
    {
        protected ServiceComponent(IDictionary<Type, Handler> handlers) : base(handlers) { }

        public override void Subscribed()
        {
            Exchange.Register(typeof(T), this);
            base.Subscribed();
        }

        public override void Detach()
        {
            Exchange.Unregister(typeof(T), this);
            base.Detach();
        }
    }
}