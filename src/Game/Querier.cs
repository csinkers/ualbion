using System;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game
{
    public class Querier : ServiceComponent<IQuerier>, IQuerier
    {
        readonly Random _random = new Random();

        public Querier()
        {
            OnAsync<QueryEvent, bool>(Query);
        }

        bool Query(QueryEvent query, Action<bool> continuation)
        {
            var context = Resolve<IEventManager>().Context;
            return InnerQuery(context, query, false, continuation);
        }

        public bool? QueryDebug(QueryEvent query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            var context = Resolve<IEventManager>().Context;
            bool result = true;
            InnerQuery(context, query, true, x => result = x);
            return result;
        }

        bool InnerQuery(EventContext context, QueryEvent query, bool debugInspect, Action<bool> continuation)
        {
            var game = Resolve<IGameState>();
            switch (query.QueryType)
            {
                case (QueryType.TemporarySwitch):      continuation(Compare(query.Operation, game.GetSwitch(query.SwitchId) ? 1 : 0, query.Immediate)); return true;
                case (QueryType.Ticker):               continuation(Compare(query.Operation, game.GetTicker(query.TickerId), query.Immediate)); return true;
                case (QueryType.CurrentMapId):         continuation(Compare(query.Operation, game.MapId.Id, query.MapId.Id)); return true;
                case (QueryType.HasEnoughGold):        continuation(Compare(query.Operation, game.Party.TotalGold, query.Argument)); return true;
                case (QueryType.InventoryHasItem): continuation(Compare(query.Operation, game.Party.GetItemCount(query.ItemId), query.Immediate)); return true;
                case (QueryType.HasPartyMember):       continuation(game.Party.StatusBarOrder.Any(x => x.Id == query.PartyMemberId)); return true;
                case (QueryType.IsPartyMemberLeader):  continuation(game.Party.Leader == query.PartyMemberId); return true;
                case (QueryType.RandomChance):         continuation(_random.Next(100) < query.Argument); return true;
                case (QueryType.TriggerType):          continuation(context.Source.Trigger == (TriggerTypes)query.Argument); return true;
                case (QueryType.ChosenVerb):    continuation(context.Source.Trigger.HasFlag((TriggerTypes)(1 << (int)query.Argument))); return true;
                case (QueryType.UsedItemId):    continuation(query.ItemId == context.Source.Id); return true;
                // Need to use QueryEvent _ instead of _ here due to a compiler issue (https://github.com/dotnet/roslyn/issues/47075)
                case (QueryType.PreviousActionResult): continuation(Resolve<IEventManager>().LastEventResult); return true;

                case (QueryType.IsScriptDebugModeActive): { continuation(false); return true; }
                case (QueryType.IsNpcActive):             { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query NpcActive")); continuation(true); return true; }
                case (QueryType.IsPartyMemberConscious):  { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query Party member conscious")); continuation(true); return true; }
                case (QueryType.EventAlreadyUsed):        { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query event already used")); continuation(false); return true; }
                case (QueryType.IsDemoVersion):           { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query is demo")); continuation(false); return true; }

                case (QueryType.PromptPlayer):
                {
                    if (context.Source == null || debugInspect)
                        return false;

                    return RaiseAsync(new YesNoPromptEvent(new StringId(query.TextSourceId, query.Argument)), continuation) > 0;
                }

                case (QueryType.PromptPlayerNumeric):
                {
                    if (context?.Source == null || debugInspect)
                        return false;

                    return RaiseAsync(
                               new NumericPromptEvent((TextId)Base.SystemText.MsgBox_EnterNumber, 0, 9999),
                               x => continuation(x == query.Argument)) > 0;
                }

                default:
                    Raise(new LogEvent(LogEvent.Level.Error, $"Unhandled query event {query} (subtype {query.QueryType}"));
                    return false;
            }
        }

        static bool Compare(QueryOperation operation, int value, int immediate) =>
            operation switch
            {
                QueryOperation.IsTrue => (value != 0),
                QueryOperation.NotEqual => (value != immediate),
                QueryOperation.OpUnk2 => (value == immediate),
                QueryOperation.Equals => (value == immediate),
                QueryOperation.GreaterThanOrEqual => (value >= immediate),
                QueryOperation.GreaterThan => (value > immediate),
                QueryOperation.OpUnk5 => (value == immediate),
                _ => true
            };
    }
}
