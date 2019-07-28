using System;
using System.Threading.Tasks;
using UAlbion.Core;

namespace UAlbion
{
    class ConsoleLogger : IComponent
    {
        EventExchange _exchange;

        public void Attach(EventExchange exchange)
        {
            _exchange = exchange;
            exchange.Subscribe<IEvent>(this);
            Task.Run(ConsoleReaderThread);
        }

        public void Receive(IEvent @event, object sender)
        {
            switch(@event)
            {
                case EngineUpdateEvent e:
                    break;

                default:
                    Console.WriteLine(@event.ToString());
                    break;
            }
        }

        public void ConsoleReaderThread()
        {
            do
            {
                var command = Console.ReadLine();
                try
                {
                    var @event = Event.Parse(command);
                    _exchange.Raise(@event, this);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Parse error: {0}", e);
                }

            } while (true);
        }
    }
}