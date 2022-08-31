using System;
using System.Globalization;
using System.Threading;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

public class EventContext
{
    static int _nextContextId;
    IEventNode _node;
    public string Id { get; } = Interlocked.Increment(ref _nextContextId).ToString(CultureInfo.InvariantCulture);
    public EventContext(EventSource source, EventContext parent)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Parent = parent;
    }

    public EventContext Parent { get; }
    public IEventSet EventSet { get; init; }
    public ushort EntryPoint { get; init; }
    public bool ClockWasRunning { get; init; }
    public Action CompletionCallback { get; init; }

    public IEventNode Node { get => _node; set { LastNode = _node; _node = value; } }
    public EventSource Source { get; }
    public EventContextStatus Status { get; set; } = EventContextStatus.Ready;
    public IEventNode LastNode { get; set; }
    public bool LastEventResult { get; set; }
    public ActionEvent LastAction { get; set; }

    public override string ToString() => $"{Id} Status:{Status} Src:{Source} Node:{Node}";
}