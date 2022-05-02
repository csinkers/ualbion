using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game;

public sealed class EventChainManager : ServiceComponent<IEventManager>, IEventManager, IDisposable
{
    static readonly EventContext BaseContext = new(new EventSource(AssetId.None, TextId.None, 0));

    readonly ThreadLocal<Stack<EventContext>> _threadContexts = new(() => new Stack<EventContext>());
    readonly HashSet<EventContext> _activeContexts = new();
    readonly StartClockEvent _startClockEvent = new();
    readonly StopClockEvent _stopClockEvent = new();
    readonly ResumeChainsEvent _resumeChainsEvent = new();

    public EventChainManager()
    {
        // Need to enqueue without a sender if we want to handle it ourselves.
        On<BeginFrameEvent>(_ => Exchange?.Enqueue(_resumeChainsEvent, null)); 
        On<ResumeChainsEvent>(ResumeChains);
        OnAsync<TriggerChainEvent>(Trigger);
    }

    public EventContext Context
    {
        get
        {
            foreach (var context in _threadContexts.Value)
                return context;
            return BaseContext;
        }
    }

    public bool LastEventResult { get; set; }
    public IEnumerable<EventContext> DebugActiveContexts => _activeContexts;

    bool HandleBoolEvent(EventContext context, IAsyncEvent<bool> boolEvent, IBranchNode branch) // Return value = whether to return.
    {
        context.Status = EventContextStatus.Waiting;
        int waiting = RaiseAsync(boolEvent, result =>
        {
#if DEBUG
            Info($"if ({context.Node.Event}) => {result}");
#endif
            context.Node = result ? branch.Next : branch.NextIfFalse;

            // If a non-query event needs to set this it will have to do it itself. This is to allow
            // things like chest / door events where different combinations of their IAsyncEvent<bool> result 
            // and the LastEventResult can mean successful opening, exiting the screen without success or a
            // trap has been triggered.
            if (boolEvent is QueryEvent) 
                LastEventResult = result;
            context.Status = EventContextStatus.Ready;
        });

        if (waiting == 0)
        {
            ApiUtil.Assert($"Async event {boolEvent} not acknowledged. Continuing immediately.");
            context.Node = context.Node.Next;
            context.Status = EventContextStatus.Ready;
        }
        else if (context.Status == EventContextStatus.Waiting)
        {
            // If the continuation hasn't been called then stop iterating for now and wait for completion.
            _threadContexts.Value.Pop();
            return true;
        }

        // If the continuation was called already then continue iterating.
        return false;
    }

    bool HandleAsyncEvent(EventContext context, IAsyncEvent asyncEvent)
    {
        context.Status = EventContextStatus.Waiting;
        int waiting = RaiseAsync(asyncEvent, () =>
        {
            context.Node = context.Node?.Next;
            context.Status = EventContextStatus.Ready;
        });

        if (waiting == 0)
        {
            ApiUtil.Assert($"Async event {asyncEvent} not acknowledged. Continuing immediately.");
            context.Node = context.Node.Next;
            context.Status = EventContextStatus.Ready;
        }
        else if (context.Status == EventContextStatus.Waiting)
        {
            _threadContexts.Value.Pop();
            return true;
        }
        // If the continuation was called already then continue iterating.
        return false;
    }

    void HandleChainCompletion(EventContext context)
    {
        if (context.ClockWasRunning)
            Raise(_startClockEvent);

        context.Status = EventContextStatus.Completing;
        context.CompletionCallback?.Invoke();
        context.Status = EventContextStatus.Complete;
        _threadContexts.Value.Pop();
        _activeContexts.Remove(context);
    }

    void Resume(EventContext context)
    {
        _threadContexts.Value.Push(context);
        context.Status = EventContextStatus.Ready;

        while (context.Node != null)
        {
            context.Status = EventContextStatus.Running;
            if (context.Node is IBranchNode branch && context.Node.Event is IAsyncEvent<bool> boolEvent)
            {
                if (HandleBoolEvent(context, boolEvent, branch))
                    return;
            }
            else if (context.Node.Event is IAsyncEvent asyncEvent)
            {
                if (HandleAsyncEvent(context, asyncEvent))
                    return;
            }
            else
            {
                Raise(context.Node.Event);
                context.Node = context.Node.Next;
            }
        }
        HandleChainCompletion(context);
    }

    void ResumeChains(ResumeChainsEvent obj)
    {
        EventContext context;
        do
        {
            context = _activeContexts.FirstOrDefault(x => x.Status == EventContextStatus.Ready);
            if (context != null)
                Resume(context);
        } while (context != null);
    }

    bool Trigger(TriggerChainEvent e, Action continuation)
    {
        var game = Resolve<IGameState>();
        if (e.ChainSource.Type == AssetType.Map && game.IsChainDisabled(e.ChainSource, e.Chain))
            return true;

        var context = new EventContext(e.Source)
        {
            Chain = e.Chain,
            Node = e.Node,
            ClockWasRunning = Resolve<IClock>().IsRunning,
            CompletionCallback = continuation,
            Status = EventContextStatus.Ready
        };

        if (context.ClockWasRunning)
            Raise(_stopClockEvent);

        _activeContexts.Add(context);
        Resume(context);
        return true;
    }

    public void Dispose() => _threadContexts?.Dispose();
}