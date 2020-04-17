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

        readonly Querier _querier;
        readonly ISet<EventContext> _activeChains = new HashSet<EventContext>();

        public IList<EventContext> ActiveContexts => new ReadOnlyCollection<EventContext>(_activeChains.ToList());

        public EventChainManager() : base(Handlers)
        {
            _querier = AttachChild(new Querier());
        }

        void RaiseWithContext(EventContext context, IEvent e)
        {
            if(e is MapEvent mapEvent)
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
                    context.Node = context.Node.NextEvent;
                    var clone = asyncEvent.CloneWithCallback(() => Resume(context));

                    RaiseWithContext(context, clone);

                    if (clone.Acknowledged)
                        return;

                    Raise(new LogEvent(LogEvent.Level.Warning, $"Async event {clone} not acknowledged. Continuing immediately."));
                }
                else
                {
                    if (context.Node is IBranchNode branch && context.Node.Event is IQueryEvent query)
                    {
                        var result = _querier.Query(context, query);
#if DEBUG
                        Raise(new LogEvent(LogEvent.Level.Info, $"if ({query}) => {result}"));
#endif
                        context.Node = result ? branch.NextEvent : branch.NextEventWhenFalse;
                    }
                    else
                    {
                        RaiseWithContext(context, context.Node.Event);
                        context.Node = context.Node.NextEvent;
                    }
                }
            }

            if (context.ClockWasRunning)
                Raise(new StartClockEvent());

            _activeChains.Remove(context);

            if (context.Parent == null) 
                return;

            if (context.Parent.Node.Event is AsyncEvent async)
                async.Complete();
            else
                Resume(context.Parent);
        }

        void Trigger(TriggerChainEvent e)
        {
            var context = new EventContext(e.Source)
            {
                Chain = e.Chain,
                Node = e.Node,
                ClockWasRunning = Resolve<IClock>().IsRunning,
                Parent = e.Context
            };

            if (context.ClockWasRunning)
                Raise(new StopClockEvent());
            Resume(context);
        }
    }
}
