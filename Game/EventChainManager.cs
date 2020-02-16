using System;
using UAlbion.Api;
using UAlbion.Core;
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

        class EventContext
        {
            public TriggerType Trigger { get; set; }
        }

        readonly EventContext _context = new EventContext();

        public EventChainManager() : base(Handlers)
        { }

        void Resume(IEventNode node)
        {
            do
            {
                if (node.Event is AsyncEvent asyncEvent)
                {
                    asyncEvent.SetCallback(() => Resume(node));
                    Raise(asyncEvent);
                    break;
                }

                if (node is IBranchNode branch && node.Event is QueryEvent query)
                {
                    node = Query(query) ? branch.NextEvent : branch.NextEventWhenFalse;
                }
                else
                {
                    Raise(node.Event);
                    node = node.NextEvent;
                }
            } while (node != null);
        }

        bool Query(QueryEvent query)
        {
            switch (query, query.SubType)
            {
                case (QueryItemEvent item, _):
                    {
                        if (item.SubType == QueryType.InventoryHasItem)
                        {
                            Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query InventoryHasItem"));
                        }
                        else if (item.SubType == QueryType.UsedItemId)
                        {
                            Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query UsedItemId"));
                        }

                        break;
                    }
                case (QueryVerbEvent verb, _):
                    {
                        if (verb.SubType == QueryType.ChosenVerb)
                        {
                            switch (verb.Verb)
                            {
                                case QueryVerbEvent.VerbType.Examine: return _context.Trigger.HasFlag(TriggerType.Examine);
                                case QueryVerbEvent.VerbType.Manipulate: return _context.Trigger.HasFlag(TriggerType.Manipulate);
                                case QueryVerbEvent.VerbType.TalkTo: return _context.Trigger.HasFlag(TriggerType.TalkTo);
                                case QueryVerbEvent.VerbType.UseItem: return _context.Trigger.HasFlag(TriggerType.UseItem);
                                default: Raise(new LogEvent(LogEvent.Level.Error, $"Unhandled query verb type {verb.Verb}")); return false;
                            }
                        }

                        Raise(new LogEvent(LogEvent.Level.Error, $"Unhandled query event {query} (subtype {query.SubType}"));
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
                case (_, QueryType.PromptPlayer):
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
                case (_, QueryType.PromptPlayerNumeric):
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query numeric prompt"));
                        break;
                    }
                default:
                    Raise(new LogEvent(LogEvent.Level.Error, $"Unhandled query event {query} (subtype {query.SubType}"));
                    break;
            }

            return true;
        }

        void Trigger(TriggerChainEvent e)
        {
            _context.Trigger = e.Trigger;
            Resume(e.Chain);
        }
    }
}
