using System;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Map;
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
            OnAsync<QueryVerbEvent, bool>(Query);
            OnAsync<QueryItemEvent, bool>(Query);
            OnAsync<PromptPlayerEvent, bool>(Query);
            OnAsync<PromptPlayerNumericEvent, bool>(Query);
            OnAsync<QueryEvent, bool>(Query);
        }

        bool Query(IQueryEvent query, Action<bool> continuation)
        {
            var context = Resolve<IEventManager>().Context;
            return InnerQuery(context, query, false, continuation);
        }

        public bool? QueryDebug(IQueryEvent query)
        {
            var context = Resolve<IEventManager>().Context;
            bool result = true;
            InnerQuery(context, query, true, x => result = x);
            return result;
        }

        bool InnerQuery(EventContext context, IQueryEvent query, bool debugInspect, Action<bool> continuation)
        {
            var game = Resolve<IGameState>();
            switch (query, query.QueryType)
            {
                case (QueryEvent q, QueryType.TemporarySwitch):      continuation(Compare(q.Operation, game.GetSwitch((SwitchId)q.Argument) ? 1 : 0, q.Immediate)); return true;
                case (QueryEvent q, QueryType.Ticker):               continuation(Compare(q.Operation, game.GetTicker((TickerId)q.Argument), q.Immediate)); return true;
                case (QueryEvent q, QueryType.CurrentMapId):         continuation(Compare(q.Operation, (int)game.MapId, q.Immediate)); return true;
                case (QueryEvent q, QueryType.HasEnoughGold):        continuation(Compare(q.Operation, game.Party.TotalGold, q.Argument)); return true;
                case (QueryItemEvent q, QueryType.InventoryHasItem): continuation(Compare(q.Operation, game.Party.GetItemCount(q.ItemId), q.Immediate)); return true;
                case (QueryEvent q, QueryType.HasPartyMember):       continuation(game.Party.StatusBarOrder.Any(x => (int)x.Id == q.Argument)); return true;
                case (QueryEvent q, QueryType.IsPartyMemberLeader):  continuation((int)game.Party.Leader == q.Argument); return true;
                case (QueryEvent q, QueryType.RandomChance):         continuation(_random.Next(100) < q.Argument); return true;
                case (QueryEvent q, QueryType.TriggerType):          continuation((ushort)context.Source.Trigger == q.Argument); return true;
                case (QueryVerbEvent verb, QueryType.ChosenVerb):    continuation(context.Source.Trigger.HasFlag((TriggerType)(1 << (int)verb.Verb))); return true;
                case (QueryItemEvent q, QueryType.UsedItemId):       continuation(context.Source is EventSource.Item item && item.ItemId == q.ItemId); return true;
                case (_, QueryType.PreviousActionResult):            continuation(Resolve<IEventManager>().LastEventResult); return true;

                case (_, QueryType.IsScriptDebugModeActive): { continuation(false); return true; }
                case (_, QueryType.IsNpcActive):             { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query NpcActive")); continuation(true); return true; }
                case (_, QueryType.IsPartyMemberConscious):  { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query Party member conscious")); continuation(true); return true; }
                case (_, QueryType.EventAlreadyUsed):        { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query event already used")); continuation(false); return true; }
                case (_, QueryType.IsDemoVersion):           { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query is demo")); continuation(false); return true; }

                case (PromptPlayerEvent prompt, QueryType.PromptPlayer):
                {
                    if (context.Source == null || debugInspect)
                        return false;

                    return RaiseAsync(new YesNoPromptEvent(prompt.TextType, prompt.TextSourceId, prompt.TextId), continuation) > 0;
                }

                case (PromptPlayerNumericEvent prompt, QueryType.PromptPlayerNumeric):
                {
                    if (context?.Source == null || debugInspect)
                        return false;

                    return RaiseAsync(
                               new NumericPromptEvent(SystemTextId.MsgBox_EnterNumber, 0, 9999),
                               x => continuation(x == prompt.Argument)) > 0;
                }

                default:
                    Raise(new LogEvent(LogEvent.Level.Error, $"Unhandled query event {query} (subtype {query.QueryType}"));
                    return false;
            }
        }

        bool Compare(QueryOperation operation, int value, int immediate) =>
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
