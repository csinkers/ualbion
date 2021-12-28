using System;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core;

public interface ILogExchange : IComponent
{
    void EnqueueEvent(IEvent e);
    event EventHandler<LogEventArgs> Log;
}