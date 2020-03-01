using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class EventChainManager : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<EventChainManager, TriggerChainEvent>((x, e) => x.Trigger(e))
        );

        class EventChainContext
        {
            public TriggerType Trigger { get; set; }
            public IEventNode Node { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public ItemId? UsedItem { get; set; }
            public bool ClockWasRunning { get; set; }
        }

        public EventChainManager() : base(Handlers) { }

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
                        var result = Query(context, query);
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

        bool Query(EventChainContext context, IQueryEvent query)
        {
            switch (query, query.QueryType)
            {
                case (QueryItemEvent item, _):
                    {
                        if (item.QueryType == QueryType.InventoryHasItem)
                        {
                            Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query InventoryHasItem"));
                        }
                        else if (item.QueryType == QueryType.UsedItemId)
                        {
                            Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query UsedItemId"));
                        }
                        break;
                    }
                case (QueryVerbEvent verb, _):
                    {
                        if (verb.QueryType == QueryType.ChosenVerb)
                        {
                            switch (verb.Verb)
                            {
                                case QueryVerbEvent.VerbType.Examine: return context.Trigger.HasFlag(TriggerType.Examine);
                                case QueryVerbEvent.VerbType.Manipulate: return context.Trigger.HasFlag(TriggerType.Manipulate);
                                case QueryVerbEvent.VerbType.TalkTo: return context.Trigger.HasFlag(TriggerType.TalkTo);
                                case QueryVerbEvent.VerbType.UseItem: return context.Trigger.HasFlag(TriggerType.UseItem);
                                default: Raise(new LogEvent(LogEvent.Level.Error, $"Unhandled query verb type {verb.Verb}")); return false;
                            }
                        }

                        Raise(new LogEvent(LogEvent.Level.Error, $"Unhandled query event {query} (subtype {query.QueryType}"));
                        break;
                    }
                case (_, QueryType.TemporarySwitch):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query TemporarySwitch"));
                        break;
                    }
                case (_, QueryType.HasPartyMember):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query HasPartyMember"));
                        break;
                    }
                case (_, QueryType.PreviousActionResult):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query PreviousActionResult"));
                        break;
                    }
                case (_, QueryType.IsScriptDebugModeActive):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query DebugModeActive"));
                        break;
                    }
                case (_, QueryType.IsNpcActive):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query NpcActive"));
                        break;
                    }
                case (_, QueryType.HasEnoughGold):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query EnoughGold"));
                        break;
                    }
                case (_, QueryType.RandomChance):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query Random"));
                        break;
                    }
                case (_, QueryType.IsPartyMemberConscious):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query Party member conscious"));
                        break;
                    }
                case (_, QueryType.IsPartyMemberLeader):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query leader check"));
                        break;
                    }
                case (_, QueryType.Ticker):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query ticker"));
                        break;
                    }
                case (_, QueryType.CurrentMapId):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query map id"));
                        break;
                    }
                case (_, QueryType.PromptPlayer): // Async
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query prompt"));
                        break;
                    }
                case (_, QueryType.TriggerType):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query trigger type"));
                        break;
                    }
                case (_, QueryType.EventAlreadyUsed):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query event already used"));
                        break;
                    }
                case (_, QueryType.IsDemoVersion):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query is demo"));
                        break;
                    }
                case (_, QueryType.PromptPlayerNumeric): // Async
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query numeric prompt"));
                        break;
                    }
                default:
                    Raise(new LogEvent(LogEvent.Level.Error, $"Unhandled query event {query} (subtype {query.QueryType}"));
                    break;
            }

            return true;
        }

        void Trigger(TriggerChainEvent e)
        {
            var context = new EventChainContext
            {
                Trigger = e.Trigger,
                Node = e.Chain,
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
