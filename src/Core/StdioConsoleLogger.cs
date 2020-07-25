using System;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class StdioConsoleLogger : Component
    {
        bool _done;

        public StdioConsoleLogger()
        {
            On<QuitEvent>(_ => IsActive = false);
            On<ClearConsoleEvent>(_ => Console.Clear());
        }

        protected override void Subscribed()
        {
            var logExchange = Resolve<ILogExchange>();
            logExchange.Log += Print;
            _done = false;
            Task.Run(ConsoleReaderThread);
        }

        protected override void Unsubscribed()
        {
            var logExchange = Resolve<ILogExchange>();
            logExchange.Log -= Print;
            _done = true;
        }

        void Print(object sender, LogEventArgs log)
        {
            Console.ForegroundColor = log.Color;
            int nesting = log.Nesting;
            if (nesting > 0)
                Console.Write(new string(' ', nesting * 2));
            Console.WriteLine(log.Message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        void ConsoleReaderThread()
        {
            var logExchange = Resolve<ILogExchange>();
            do
            {
                var command = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(command))
                    continue;

                try
                {
                    var @event = Event.Parse(command);
                    if (@event != null)
                        logExchange.EnqueueEvent(@event);
                    else
                        Console.WriteLine("Unknown event \"{0}\"", command);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Parse error: {0}", e);
                }

            } while (!_done);
        }
    }
}
