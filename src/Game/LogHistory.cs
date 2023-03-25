using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;

namespace UAlbion.Game;

public class LogHistory : ServiceComponent<ILogHistory>, ILogHistory
{
    const int MaxHistory = 1000;
    readonly object _syncRoot = new();
    readonly Queue<LogEventArgs> _history = new();

    public LogHistory()
    {
        On<ClearConsoleEvent>(_ =>
        {
            lock (_syncRoot)
                _history.Clear();
        });
    }
    protected override void Subscribed()
    {
        var logExchange = Resolve<ILogExchange>();
        logExchange.Log += AddLine;
    }

    protected override void Unsubscribed()
    {
        var logExchange = Resolve<ILogExchange>();
        logExchange.Log -= AddLine;
    }

    void AddLine(object sender, LogEventArgs e)
    {
        lock (_syncRoot)
        {
            _history.Enqueue(e);
            if (_history.Count > MaxHistory)
                _history.Dequeue();
        }
    }

    public void Access<T>(T context, Action<T, IReadOnlyCollection<LogEventArgs>> operation)
    {
        if (operation == null) throw new ArgumentNullException(nameof(operation));
        lock (_syncRoot)
            operation(context, _history);
    }
}