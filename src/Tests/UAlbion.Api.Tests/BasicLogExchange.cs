using System;
using UAlbion.Api.Eventing;

namespace UAlbion.Api.Tests;

public class BasicLogExchange : ILogExchange
{
    public void Attach(EventExchange exchange) { }
    public void Remove() { }

    public void Receive(IEvent e, object sender)
    {
        Log?.Invoke(this, new LogEventArgs
        {
            Time = DateTime.Now,
            Nesting = 0,
            Message = e.ToString(),
            Color = Console.ForegroundColor = ConsoleColor.Gray,
        });
    }
    public bool IsActive { get; set; }
    public int ComponentId => -1;
    public void EnqueueEvent(IEvent e) { }
    public event EventHandler<LogEventArgs> Log;
}