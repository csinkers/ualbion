using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class ConsoleLogger : IComponent
    {
        readonly ConcurrentQueue<IEvent> _queuedEvents = new ConcurrentQueue<IEvent>();
        LogEvent.Level _logLevel = LogEvent.Level.Info;
        EventExchange _exchange;
        bool _done;

        public void Attach(EventExchange exchange)
        {
            _exchange = exchange;
            // Only need to subscribe to verbose events, as all non-verbose events will be delivered
            // here anyway as long as this was given to Engine as the logger component.
            exchange.Subscribe<BeginFrameEvent>(this);
            exchange.Subscribe<SetLogLevelEvent>(this);
            Task.Run(ConsoleReaderThread);
        }
        public void Detach() => _exchange = null;
        public void Subscribed() { }
        public bool IsSubscribed => _exchange != null;
        public void Receive(IEvent @event, object sender)
        {
            switch(@event)
            {
                case BeginFrameEvent _:
                    while (_queuedEvents.TryDequeue(out var queuedEvent))
                    {
                        try { _exchange.Raise(queuedEvent, this); }
                        catch (Exception exception) { Console.WriteLine("Error: {0}", exception.Message); }
                    }
                    break;

                case SetLogLevelEvent e:
                    _logLevel = e.Level;
                    break;

                case IVerboseEvent _: break;
                case LogEvent e:
                {
                    if (e.Severity < _logLevel)
                        break;

                    switch (e.Severity)
                    {
                        case LogEvent.Level.Verbose:
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                        case LogEvent.Level.Warning:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case LogEvent.Level.Error:
                        case LogEvent.Level.Critical:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    }

                    int nesting = _exchange.Nesting;
                    if(nesting > 0)
                        Console.Write(new string(' ', nesting * 2));
                    Console.WriteLine(e.Message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                }

                case ClearConsoleEvent _ :
                    Console.Clear();
                    break;

                case QuitEvent _:
                    _done = true;
                    break;

                case HelpEvent helpEvent:
                {
                    if (string.IsNullOrEmpty(helpEvent.CommandName))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Command Usage Help:");
                        Console.WriteLine("-------------------------------------");
                        PrintHelpSummary(Event.GetEventMetadata());
                        Console.WriteLine();
                    }
                    else
                    {
                        PrintHelp(helpEvent.CommandName);
                    }
                    break;
                }
                case WhoEvent whoEvent:
                    PrintEventConsumers(whoEvent.CommandName);
                    break;

                default:
                {
                    if (sender == this || _logLevel > LogEvent.Level.Info) return;
                    int nesting = _exchange.Nesting;
                    if(nesting > 0)
                        Console.Write(new string(' ', nesting * 2));
                    Console.WriteLine(@event.ToString());
                    break;
                }
            }
        }

        void PrintHelp(string pattern)
        {
            Console.WriteLine();
            var metadata = Event.GetEventMetadata()
                .FirstOrDefault(x => x.Name.Equals(pattern, StringComparison.InvariantCultureIgnoreCase)
                || x.Aliases != null &&
                   x.Aliases.Any(y => y.Equals(pattern, StringComparison.InvariantCultureIgnoreCase)));

            if (metadata != null)
            {
                PrintDetailedHelp(metadata);
            }
            else
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                var matchingEvents = Event.GetEventMetadata().Where(x => regex.IsMatch(x.Name)).ToList();

                if (matchingEvents.Any())
                    PrintHelpSummary(matchingEvents);
                else
                    Console.WriteLine("The command \"{0}\" is not recognised.", pattern);
            }
        }

        void PrintHelpSummary(IEnumerable<EventMetadata> events)
        {
            foreach (var e in events)
            {
                var paramList = e.Parts.Length == 0
                    ? ""
                    : " " + string.Join(" ",
                        e.Parts.Select(x => x.IsOptional ? $"[{x.Name}]" : x.Name));

                Console.WriteLine("    {0}{1}: {2}", e.Name, paramList, e.HelpText);
            }
        }

        void PrintDetailedHelp(EventMetadata metadata)
        {
            var paramList = metadata.Parts.Length == 0
                ? ""
                : " " + string.Join(" ",
                    metadata.Parts.Select(x => x.IsOptional ? $"[{x.Name}]" : x.Name));

            Console.WriteLine("    {0}{1}: {2}", metadata.Name, paramList, metadata.HelpText);
            foreach (var param in metadata.Parts)
                Console.WriteLine("        {0} ({1}): {2}", param.Name, param.PropertyType, param.HelpText);
        }

        void PrintEventConsumers(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return;

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var matchingEvents = Event.GetEventMetadata()
                .Where(x => regex.IsMatch(x.Name))
                .ToList();

            foreach (var e in matchingEvents)
            {
                Console.Write("    ");
                Console.WriteLine(e.Name);
                foreach (var recipient in _exchange.EnumerateRecipients(e.Type))
                {
                    Console.Write("        ");
                    Console.WriteLine(recipient);
                }
            }

            var eventsByTypeName = Event.AllEventTypes
                .Where(x =>
                    x.FullName != null &&
                    regex.IsMatch(x.FullName) &&
                    matchingEvents.All(y => y.Type != x));

            foreach (var e in eventsByTypeName)
            {
                Console.Write("    ");
                Console.WriteLine(e.Name);
                foreach (var recipient in _exchange.EnumerateRecipients(e))
                {
                    Console.Write("        ");
                    Console.WriteLine(recipient);
                }
            }
        }

        void ConsoleReaderThread()
        {
            do
            {
                var command = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(command))
                    continue;

                try
                {
                    var @event = Event.Parse(command);
                    if (@event != null)
                    {
                        if(@event is AsyncEvent async)
                            @event = async.CloneWithCallback(() => Console.WriteLine($"Async event \"{async}\" completed."));

                        _queuedEvents.Enqueue(@event);
                    }
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
