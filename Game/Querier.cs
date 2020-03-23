using System;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.State;

namespace UAlbion.Game
{
    public class Querier : ServiceComponent<IQuerier>, IQuerier
    {
        readonly Random _random = new Random();
        public Querier() : base(null) { }
        public bool Query(EventContext context, IQueryEvent query)
        {
            var game = Resolve<IGameState>();
            switch (query, query.QueryType)
            {
                case (QueryEvent q, QueryType.TemporarySwitch):      return Compare(q.Operation, game.GetSwitch(q.Argument), q.Immediate);
                case (QueryEvent q, QueryType.Ticker):               return Compare(q.Operation, game.GetTicker(q.Argument), q.Immediate);
                case (QueryEvent q, QueryType.CurrentMapId):         return Compare(q.Operation, (int)game.MapId, q.Immediate);
                case (QueryEvent q, QueryType.HasEnoughGold):        return Compare(q.Operation, game.Party.TotalGold, q.Argument);
                case (QueryItemEvent q, QueryType.InventoryHasItem): return Compare(q.Operation, game.Party.GetItemCount(q.ItemId), q.Immediate);
                case (QueryEvent q, QueryType.HasPartyMember):       return game.Party.StatusBarOrder.Any(x => (int)x.Id == q.Argument);
                case (QueryEvent q, QueryType.IsPartyMemberLeader):  return (int)game.Party.Leader == q.Argument;
                case (QueryEvent q, QueryType.RandomChance):         return _random.Next(100) < q.Argument;
                case (QueryEvent q, QueryType.TriggerType):          return (ushort)context.Trigger == q.Argument;
                case (QueryVerbEvent verb, QueryType.ChosenVerb):    return context.Trigger.HasFlag((TriggerType)(1 << (int)verb.Verb));
                case (QueryItemEvent q, QueryType.UsedItemId):       return context.UsedItem == q.ItemId;
                case (_, QueryType.PreviousActionResult):            return context.LastEventResult;

                case (_, QueryType.IsScriptDebugModeActive): { return false; }
                case (_, QueryType.IsNpcActive):             { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query NpcActive")); break; }
                case (_, QueryType.IsPartyMemberConscious):  { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query Party member conscious")); break; }
                case (_, QueryType.EventAlreadyUsed):        { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query event already used")); break; }
                case (_, QueryType.IsDemoVersion):           { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query is demo")); return false; }
                case (_, QueryType.PromptPlayer):            { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query prompt")); break; } // Async
                case (_, QueryType.PromptPlayerNumeric):     { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query numeric prompt")); break; } // Async

                default:
                    Raise(new LogEvent(LogEvent.Level.Error, $"Unhandled query event {query} (subtype {query.QueryType}"));
                    break;
            }

            return true;
        }

        bool Compare(QueryOperation operation, int value, int immediate) =>
            operation switch
            {
                QueryOperation.Unk0 => (value == immediate),
                QueryOperation.NotEqual => (value != immediate),
                QueryOperation.Unk2 => (value == immediate),
                QueryOperation.Equals => (value == immediate),
                QueryOperation.Unk3 => (value == immediate),
                QueryOperation.GreaterThan => (value > immediate),
                QueryOperation.Unk5 => (value == immediate),
                _ => true
            };
    }
}
