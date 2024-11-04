using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Diag;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game;

public sealed class EventChainManager : ServiceComponent<IEventManager>, IEventManager, IDisposable
{
    readonly List<EventContext> _contexts = new();
    readonly List<Breakpoint> _breakpoints = new();
    public int CurrentDebugContextIndex { get; set; } = -1;
    public EventContext CurrentDebugContext =>
        CurrentDebugContextIndex >= 0 && CurrentDebugContextIndex < _contexts.Count
            ? _contexts[CurrentDebugContextIndex]
            : null;

    public IReadOnlyList<EventContext> Contexts => _contexts;
    public IReadOnlyList<Breakpoint> Breakpoints => _breakpoints;

    public EventChainManager()
    {
        OnAsync<TriggerChainEvent>(Trigger);
        Context = new EventContext(new EventSource(AssetId.None, 0), null);
    }

    public void AddBreakpoint(Breakpoint bp) => _breakpoints.Add(bp);
    public void RemoveBreakpoint(int index) => _breakpoints.RemoveAt(index);
    public void ContinueExecution(EventContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Status == EventContextStatus.Breakpoint)
            context.Status = EventContextStatus.Ready;
    }

    public void SingleStep(EventContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Status == EventContextStatus.Breakpoint)
            context.Status = EventContextStatus.Ready;

        context.BreakOnReturn = true;
    }

    AlbionTask Trigger(TriggerChainEvent e)
    {
        // If the event chain is from a map, check if it's already been disabled in the game state
        var game = Resolve<IGameState>();
        if (e.EventSet.Id.Type == AssetType.Map && game.IsChainDisabled(e.EventSet.Id, e.EventSet.GetChainForEvent(e.EntryPoint)))
        {
            return AlbionTask.CompletedTask;
        }

        var wasClockRunning = Resolve<IClock>().IsRunning;
        var firstNode = e.EventSet.Events[e.EntryPoint];
        var context = new EventContext(e.Source, (EventContext)Context)
        {
            EntryPoint = e.EntryPoint,
            EventSet = e.EventSet,
            Node = firstNode,
            ClockWasRunning = wasClockRunning,
            LastAction = firstNode.Event as ActionEvent
        };

        if (wasClockRunning)
            Raise(StopClockEvent.Instance);

        ReadyContext(context);
        _contexts.Add(context);
        var task = Resume(context);
#if DEBUG
        if (!task.IsCompleted)
            task.Named($"ECM.Trigger C{context.Id}: {e}");
#endif
        return task;
    }

    void ReadyContext(EventContext context)
    {
        var breakpointHit = false;
        if (context.BreakOnReturn)
        {
            context.BreakOnReturn = false;
            breakpointHit = true;
        }
        else
        {
            foreach (var breakpoint in _breakpoints)
            {
                if (!breakpoint.Target.IsNone && breakpoint.Target != context.EventSet.Id)
                    continue;

                if (breakpoint.TriggerType.HasValue && breakpoint.TriggerType.Value != context.Source.Trigger)
                    continue;

                if (breakpoint.EventId.HasValue && breakpoint.EventId.Value != context.Node?.Id)
                    continue;

                breakpointHit = true;
                break;
            }
        }

        context.Status = breakpointHit ? EventContextStatus.Breakpoint : EventContextStatus.Ready;
    }

    async AlbionTask Resume(EventContext context)
    {
        if (context.Status != EventContextStatus.Ready)
            return;

        var oldContext = Context;
        Context = context; // Set thread-local context for all components

        while (context.Node != null)
        {
            var node = context.Node;
            context.Status = EventContextStatus.Running;
            AlbionTask task;
            if (node is IBranchNode branch && node.Event is IQueryEvent<bool> boolEvent)
                task = HandleBoolEvent(context, boolEvent, branch).AsUntyped;
            else
                task = HandleAsyncEvent(context, node.Event);

#if DEBUG
            _ = task.Named($"ECM.Resume for C{context.Id} {context.EventSet.Id}:{node.Id}: {node.Event}");
#endif
            await task;
        }

        context.Status = EventContextStatus.Completing;

        if (context.ClockWasRunning)
            Raise(StartClockEvent.Instance);

        _contexts.Remove(context);

        if (context.Parent != null)
            Context = context.Parent;

        context.Status = EventContextStatus.Complete;
    }

#pragma warning disable CA1508 // Avoid dead conditional code
// context.Status can be modified due to the RaiseAsync calls, but the code analysis isn't figuring it out so
// it was flagging the "context.Status == Waiting" check as always true.
    async AlbionTask HandleAsyncEvent(EventContext context, IEvent asyncEvent)
    {
        context.Status = EventContextStatus.Waiting;

        var task = RaiseA(asyncEvent);
#if DEBUG
            _ = task.Named($"ECM.HandleAsyncEvent for C{context.Id} {context.EventSet.Id}:{context.Node.Id}: {context.Node.Event}");
#endif
        await task;

        context.Node = context.Node?.Next;
        ReadyContext(context);
    }

    async AlbionTask<bool> HandleBoolEvent(EventContext context, IQueryEvent<bool> boolEvent, IBranchNode branch) // Return value = whether to return.
    {
        context.Status = EventContextStatus.Waiting;
        var task = RaiseQueryA(boolEvent);
#if DEBUG
            _ = task.Named($"ECM.HandleBoolEvent for C{context.Id} {context.EventSet.Id}:{context.Node.Id}: {context.Node.Event}");
#endif

        var result = await task;

#if DEBUG
        // Info($"if ({context.Node.Event}) => {result}");
#endif
        context.Node = result ? branch.Next : branch.NextIfFalse;

        // If a non-query event needs to set this it will have to do it itself. This is to allow
        // things like chest / door events where different combinations of their IAsyncEvent<bool> result 
        // and the LastEventResult can mean successful opening, exiting the screen without success or a
        // trap has been triggered.
        if (boolEvent is QueryEvent)
            context.LastEventResult = result;

        ReadyContext(context);
        return result;
    }
#pragma warning restore CA1508 // Avoid dead conditional code

    public void Dispose() { }
}