using System;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public interface ILogExchange : IComponent
    {
        void EnqueueEvent(IEvent e);
        event EventHandler<LogEventArgs> Log;
    }
}