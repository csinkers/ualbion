using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public interface IEventManager
    {
        IList<EventContext> ActiveContexts { get; }
    }

    public class EventChainManager : ServiceComponent<IEventManager>, IEventManager
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<EventChainManager, TriggerChainEvent>((x, e) => x.Trigger(e))
        );

        readonly ISet<EventContext> _activeChains = new HashSet<EventContext>();

        public IList<EventContext> ActiveContexts => new ReadOnlyCollection<EventContext>(_activeChains.ToList());

        public EventChainManager() : base(Handlers) { }

        void RaiseWithContext(EventContext context, IEvent e)
        {
            if(e is IContextualEvent mapEvent)
                mapEvent.Context = context;
            Raise(e);
        }

        void Resume(EventContext context)
        {
            _activeChains.Add(context);
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

                    RaiseWithContext(context, clone);

                    if (clone.Acknowledged)
                        return;

                    Raise(new LogEvent(LogEvent.Level.Warning, $"Async event {clone} not acknowledged. Continuing immediately."));
                }
                else
                {
                    RaiseWithContext(context, context.Node.Event);

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

            _activeChains.Remove(context);
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
