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
    class ResumeChainsEvent : Event, IVerboseEvent { }
    static readonly ResumeChainsEvent ResumeChainsInstance = new();
    static readonly StartClockEvent StartClockEvent = new();
    static readonly StopClockEvent StopClockEvent = new();

    readonly List<EventContext> _contexts = new();
    readonly List<Breakpoint> _breakpoints = new();
    public EventContext CurrentDebugContext { get; }
    public IReadOnlyList<EventContext> Contexts => _contexts;
    public IReadOnlyList<Breakpoint> Breakpoints => _breakpoints;

    public EventChainManager()
    {
        // Need to enqueue without a sender if we want to handle it ourselves.
        On<BeginFrameEvent>(_ => Exchange?.Enqueue(ResumeChainsInstance, null)); 
        On<ResumeChainsEvent>(ResumeChains);
        OnAsync<TriggerChainEvent>(Trigger);
        Context = new EventContext(new EventSource(AssetId.None, 0), null);
    }

    public void AddBreakpoint(Breakpoint bp) => _breakpoints.Add(bp);
    public void RemoveBreakpoint(int index) => _breakpoints.RemoveAt(index);
    public void ContinueExecution(EventContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (context.Status == EventContextStatus.Breakpoint)
            context.Status = EventContextStatus.Ready;
    }

    public void SingleStep(EventContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (context.Status == EventContextStatus.Breakpoint)
            context.Status = EventContextStatus.Ready;

        context.BreakOnReturn = true;
    }

    async AlbionTask Trigger(TriggerChainEvent e)
    {
        // If the event chain is from a map, check if it's already been disabled in the game state
        var game = Resolve<IGameState>();
        if (e.EventSet.Id.Type == AssetType.Map && game.IsChainDisabled(e.EventSet.Id, e.EventSet.GetChainForEvent(e.EntryPoint)))
        {
            return;
        }

        var isClockRunning = Resolve<IClock>().IsRunning;
        var firstNode = e.EventSet.Events[e.EntryPoint];
        var context = new EventContext(e.Source, (EventContext)Context)
        {
            EntryPoint = e.EntryPoint,
            EventSet = e.EventSet,
            Node = firstNode,
            ClockWasRunning = isClockRunning,
            LastAction = firstNode.Event as ActionEvent
        };

        if (isClockRunning)
            Raise(StopClockEvent);

        ReadyContext(context);
        _contexts.Add(context);
        await Resume(context);
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

    void ResumeChains(ResumeChainsEvent _)
    {
        /*
        bool ran;
        do
        {
            ran = false;
            foreach (var context in _contexts)
            {
                if (context.Status == EventContextStatus.Ready)
                {
                    Resume(context);
                    ran = true;
                    break;
                }
            }
        } while (ran);
        */
    }

    async AlbionTask Resume(EventContext context)
    {
        if (context.Status != EventContextStatus.Ready)
            return;

        var oldContext = Context;
        Context = context; // Set thread-local context for all components

        while (context.Node != null)
        {
            context.Status = EventContextStatus.Running;
            if (context.Node is IBranchNode branch && context.Node.Event is IQueryEvent<bool> boolEvent)
                await HandleBoolEvent(context, boolEvent, branch);
            else
                await HandleAsyncEvent(context, context.Node.Event);
        }

        context.Status = EventContextStatus.Completing;

        if (context.ClockWasRunning)
            Raise(StartClockEvent);

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
        await RaiseAsync(asyncEvent);
        context.Node = context.Node?.Next;
        ReadyContext(context);
    }

    async AlbionTask<bool> HandleBoolEvent(EventContext context, IQueryEvent<bool> boolEvent, IBranchNode branch) // Return value = whether to return.
    {
        context.Status = EventContextStatus.Waiting;
        var result = await RaiseQueryAsync(boolEvent);

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