using System;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class StdioConsoleLogger : Component
    {
        public StdioConsoleLogger()
        {
            On<QuitEvent>(_ => IsActive = false);
            On<ClearConsoleEvent>(_ => Console.Clear());
        }

        protected override void Subscribed()
        {
            var logExchange = Resolve<ILogExchange>();
            logExchange.Log += Print;
        }

        protected override void Unsubscribed()
        {
            var logExchange = Resolve<ILogExchange>();
            logExchange.Log -= Print;
        }

        static void Print(object sender, LogEventArgs log)
        {
            Console.ForegroundColor = log.Color;
            int nesting = log.Nesting;
            if (nesting > 0)
                Console.Write(new string(' ', nesting * 2));
            Console.WriteLine(log.Message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
