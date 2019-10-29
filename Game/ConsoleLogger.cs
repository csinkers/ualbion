using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class ConsoleLogger : IComponent
    {
        readonly ConcurrentQueue<IEvent> _queuedEvents = new ConcurrentQueue<IEvent>();
        EventExchange _exchange;
        bool _done;

        public void Attach(EventExchange exchange)
        {
            _exchange = exchange;
            // Only need to subscribe to verbose events, as all non-verbose events will be delivered
            // here anyway as long as this was given to Engine as the logger component.
            exchange.Subscribe<BeginFrameEvent>(this);
            Task.Run(ConsoleReaderThread);
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

                case IVerboseEvent _: break;
                case LogEvent e:
                {
                    switch ((LogEvent.Level)e.Severity)
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

                    int nesting = EventExchange.Nesting;
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
                        Console.WriteLine();
                        var metadata = Event.GetEventMetadata()
                            .FirstOrDefault(x => x.Name.Equals(helpEvent.CommandName, StringComparison.InvariantCultureIgnoreCase)
                            || x.Aliases != null && 
                               x.Aliases.Any(y => y.Equals(helpEvent.CommandName, StringComparison.InvariantCultureIgnoreCase)));

                        if (metadata != null)
                        {
                            PrintDetailedHelp(metadata);
                        }
                        else
                        {
                            var regex = new Regex(helpEvent.CommandName);
                            var matchingEvents = Event.GetEventMetadata().Where(x => regex.IsMatch(x.Name)).ToList();

                            if(matchingEvents.Any())
                                PrintHelpSummary(matchingEvents);
                            else
                                Console.WriteLine("The command \"{0}\" is not recognised.", helpEvent.CommandName);
                        }
                    }
                    break;

                default:
                { 
                    if (sender == this) return;
                    int nesting = EventExchange.Nesting;
                    if(nesting > 0)
                        Console.Write(new string(' ', nesting * 2));
                    Console.WriteLine(@event.ToString());
                    break;
                }
            }
        }

        public void Detach()
        {
            _exchange = null;
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
                        _queuedEvents.Enqueue(@event);
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
