using System;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;

namespace UAlbion.Core;

public class StdioConsoleReader : Component
{
    bool _done;

    public StdioConsoleReader() => On<QuitEvent>(_ => IsActive = false);

    protected override void Subscribed()
    {
        _done = false;
        Task.Run(ConsoleReaderThread);
    }

    protected override void Unsubscribed()
    {
        _done = true;
    }

    void ConsoleReaderThread()
    {
        var logExchange = Resolve<ILogExchange>();
        do
        {
            var command = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(command))
                continue;

            var @event = Event.Parse(command);
            if (@event != null)
                logExchange.EnqueueEvent(@event);
            else
                Console.WriteLine("Unknown event \"{0}\"", command);
        } while (!_done);
    }
}