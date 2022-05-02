using System;

namespace UAlbion.Api.Eventing;

public interface ILogExchange : IComponent
{
    void EnqueueEvent(IEvent e);
    event EventHandler<LogEventArgs> Log;
}