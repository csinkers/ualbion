using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class EventChainManager : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<EventChainManager, TriggerChainEvent>((x, e) => x.Trigger(e))
        );

        readonly Querier _querier;

        public EventChainManager() : base(Handlers)
        {
            _querier = AttachChild(new Querier());
        }

        void RaiseWithContext(EventChainContext context, IEvent e)
        {
            if(e is IPositionedEvent positioned)
                Raise(positioned.OffsetClone(context.X, context.Y));
            else 
                Raise(e);
        }

        void Resume(EventChainContext context)
        {
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

            if(context.ClockWasRunning)
                Raise(new StartClockEvent());
        }

        void Trigger(TriggerChainEvent e)
        {
            var context = new EventChainContext
            {
                Trigger = e.Trigger,
                Node = e.Chain.Events[0],
                X = e.X,
                Y = e.Y,
                ClockWasRunning = Resolve<IClock>().IsRunning
            };

            if (context.ClockWasRunning)
                Raise(new StopClockEvent());
            Resume(context);
        }
    }
}
