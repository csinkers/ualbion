using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class EventChainManager : ServiceComponent<IEventManager>, IEventManager
    {
        readonly ThreadLocal<Stack<EventContext>> _activeContext = new ThreadLocal<Stack<EventContext>>(() => new Stack<EventContext>());
        readonly HashSet<EventContext> _pendingAsyncContexts = new HashSet<EventContext>();

        public EventChainManager()
        {
            On<TriggerChainEvent>(Trigger);
        }

        public EventContext Context => _activeContext.Value.FirstOrDefault();

        void Resume(EventContext context)
        {
            _activeContext.Value.Push(context);
            _pendingAsyncContexts.Remove(context);

            while (context.Node != null)
            {
                if (context.Node.Event is AsyncEvent asyncEvent)
                {
                    var clone = asyncEvent.CloneWithCallback(() =>
                    {
                        if (context.Node is IBranchNode branch)
                        {
#if DEBUG
                            Raise(new LogEvent(LogEvent.Level.Info, $"if ({context.Node.Event}) => {context.LastEventResult}"));
#endif
                            context.Node = context.LastEventResult ? branch.NextEvent : branch.NextEventWhenFalse;
                        }
                        else context.Node = context.Node.NextEvent;

                        Resume(context);
                    });

                    Raise(clone);

                    switch (clone.AsyncStatus)
                    {
                        case AsyncStatus.Unacknowledged:
                            Raise(new LogEvent(LogEvent.Level.Warning, $"Async event {clone} not acknowledged. Continuing immediately."));
                            break;
                        case AsyncStatus.Acknowledged: // Callback will be called later on so return for now
                            _activeContext.Value.Pop();
                            _pendingAsyncContexts.Add(context);
                            return;
                        case AsyncStatus.Complete: // Completed asynchronously, keep processing events in the chain
                            break;
                    }

                    context.Node = context.Node.NextEvent;
                }
                else
                {
                    Raise(context.Node.Event);

                    if (context.Node is IBranchNode branch)
                    {
#if DEBUG
                        Raise(new LogEvent(LogEvent.Level.Info, $"if ({context.Node.Event}) => {context.LastEventResult}"));
#endif
                        context.Node = context.LastEventResult ? branch.NextEvent : branch.NextEventWhenFalse;
                    }
                    else context.Node = context.Node.NextEvent;
                }
            }

            if (context.ClockWasRunning)
                Raise(new StartClockEvent());

            context.CompletionCallback?.Invoke();
        }

        void Trigger(TriggerChainEvent e)
        {
            var context = new EventContext(e.Source)
            {
                Chain = e.Chain,
                Node = e.Node,
                ClockWasRunning = Resolve<IClock>().IsRunning,
                CompletionCallback = e.Complete
            };

            if (context.ClockWasRunning)
                Raise(new StopClockEvent());
            Resume(context);
        }
    }
}
