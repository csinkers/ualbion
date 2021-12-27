using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class LogExchange : ILogExchange
    {
        readonly ConcurrentQueue<IEvent> _queuedEvents = new();
        LogLevel _logLevel = LogLevel.Info;
        EventExchange _exchange;

        public bool IsActive { get; set; } // Dummy implementation, value not currently used.
        public void EnqueueEvent(IEvent @event) => _queuedEvents.Enqueue(@event);
        public event EventHandler<LogEventArgs> Log;

        public void Attach(EventExchange exchange)
        {
            _exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
            // Only need to subscribe to verbose events, as all non-verbose events will be delivered
            // here anyway as long as this was given to Engine as the logger component.
            exchange.Subscribe<BeginFrameEvent>(this);
            exchange.Subscribe<SetLogLevelEvent>(this);
            exchange.Register<ILogExchange>(this);
            IsActive = true;
        }

        public void Remove()
        {
            _exchange.Unregister(this);
            _exchange.Unsubscribe(this);
            _exchange = null;
        }

        public void Receive(IEvent @event, object sender)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            bool highlight = @event is IHighlightEvent;
            switch(@event)
            {
                case BeginFrameEvent _:
                    while (_queuedEvents.TryDequeue(out var queuedEvent))
                    {
                        try
                        {
                            switch (queuedEvent)
                            {
                                case IAsyncEvent<bool> boolEvent:
                                    _exchange.RaiseAsync(boolEvent, this, x => { Console.WriteLine($"{boolEvent}: {x}"); });
                                    break;
                                case IAsyncEvent<int> intEvent:
                                    _exchange.RaiseAsync(intEvent, this, x => { Console.WriteLine($"{intEvent}: {x}"); });
                                    break;
                                default:
                                    _exchange.Raise(queuedEvent, this);
                                    break;
                            }
                        }
                        catch (Exception exception) { Console.WriteLine("Error: {0}", exception.Message); }
                    }
                    break;

                case ClearConsoleEvent _: break; // Handled by loggers directly
                case SetLogLevelEvent e:
                    _logLevel = e.Level;
                    break;

                case IVerboseEvent _: break;
                case LogEvent e:
                {
                    if (e.Severity < _logLevel)
                        break;

                    Log?.Invoke(this, new LogEventArgs
                    {
                        Time = DateTime.Now,
                        Nesting = _exchange.Nesting,
                        Message = e.Message,
                        Color =
                            e.Severity switch
                                {
                                LogLevel.Critical => Console.ForegroundColor = ConsoleColor.Red,
                                LogLevel.Error => Console.ForegroundColor = ConsoleColor.Red,
                                LogLevel.Warning => Console.ForegroundColor = ConsoleColor.Yellow,
                                _ => Console.ForegroundColor = ConsoleColor.Gray,
                                }
                    });
                    break;
                }

                case HelpEvent helpEvent:
                {
                    var sb = new StringBuilder();
                    if (string.IsNullOrEmpty(helpEvent.CommandName))
                    {
                        sb.AppendLine();
                        sb.AppendLine("Command Usage Help:");
                        sb.AppendLine("-------------------------------------");
                        PrintHelpSummary(sb, Event.GetEventMetadata());
                        sb.AppendLine();
                    }
                    else
                    {
                        PrintHelp(sb, helpEvent.CommandName);
                    }

                    Log?.Invoke(this, new LogEventArgs
                    {
                        Time = DateTime.Now,
                        Color = ConsoleColor.Gray,
                        Message = sb.ToString()
                    });
                    break;
                }
                case WhoEvent whoEvent:
                {
                    var sb = new StringBuilder();
                    PrintEventConsumers(sb, whoEvent.CommandName);
                    Log?.Invoke(this, new LogEventArgs
                    {
                        Time = DateTime.Now,
                        Color = ConsoleColor.Gray,
                        Message = sb.ToString()
                    });
                    break;
                }

                default:
                {
                    if (sender == this || (!highlight && _logLevel > LogLevel.Info))
                        return;

                    Log?.Invoke(this, new LogEventArgs
                    {
                        Time = DateTime.Now,
                        Color = highlight ? ConsoleColor.Cyan : ConsoleColor.Gray,
                        Nesting = _exchange.Nesting,
                        Message = @event.ToString()
                    });

                    break;
                }
            }
        }

        void PrintHelp(StringBuilder sb, string pattern)
        {
            sb.AppendLine();
            var metadata = Event.GetEventMetadata()
                .FirstOrDefault(x => x.Name.Equals(pattern, StringComparison.InvariantCultureIgnoreCase)
                                     || x.Aliases != null &&
                                     x.Aliases.Any(y => y.Equals(pattern, StringComparison.InvariantCultureIgnoreCase)));

            if (metadata != null)
            {
                PrintDetailedHelp(sb, metadata);
            }
            else
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                var matchingEvents = Event.GetEventMetadata().Where(x => regex.IsMatch(x.Name)).ToList();

                if (matchingEvents.Any())
                    PrintHelpSummary(sb, matchingEvents);
                else
                    sb.AppendFormat(CultureInfo.InvariantCulture, "The command \"{0}\" is not recognised." + Environment.NewLine, pattern);
            }
        }

        static void PrintHelpSummary(StringBuilder sb, IEnumerable<EventMetadata> events)
        {
            foreach (var e in events)
            {
                var paramList = e.Parts.Count == 0
                    ? ""
                    : " " + string.Join(" ",
                          e.Parts.Select(x => x.IsOptional ? $"[{x.Name}]" : x.Name));

                sb.AppendFormat(CultureInfo.InvariantCulture, "    {0}{1}: {2}" + Environment.NewLine, e.Name, paramList, e.HelpText);
            }
        }

        public static void PrintDetailedHelp(StringBuilder sb, EventMetadata metadata)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            var paramList = metadata.Parts.Count == 0
                ? ""
                : " " + string.Join(" ",
                      metadata.Parts.Select(x => x.IsOptional ? $"[{x.Name}]" : x.Name));

            sb.AppendFormat(CultureInfo.InvariantCulture, "    {0}{1}: {2}" + Environment.NewLine, metadata.Name, paramList, metadata.HelpText);
            foreach (var param in metadata.Parts)
                sb.AppendFormat(CultureInfo.InvariantCulture, "        {0} ({1}): {2}" + Environment.NewLine, param.Name, param.PropertyType, param.HelpText);
        }

        void PrintEventConsumers(StringBuilder sb, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return;

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var matchingEvents = Event.GetEventMetadata()
                .Where(x => regex.IsMatch(x.Name))
                .ToList();

            foreach (var e in matchingEvents)
            {
                sb.Append("    ");
                sb.AppendLine(e.Name);
                foreach (var recipient in _exchange.EnumerateRecipients(e.Type))
                {
                    sb.Append("        ");
                    sb.AppendLine(recipient.ToString());
                }
            }

            var eventsByTypeName = Event.AllEventTypes
                .Where(x =>
                    x.FullName != null &&
                    regex.IsMatch(x.FullName) &&
                    matchingEvents.All(y => y.Type != x));

            foreach (var e in eventsByTypeName)
            {
                sb.Append("    ");
                sb.AppendLine(e.Name);
                foreach (var recipient in _exchange.EnumerateRecipients(e))
                {
                    sb.Append("        ");
                    sb.AppendLine(recipient.ToString());
                }
            }
        }
    }
}