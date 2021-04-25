using System;
using System.Linq;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game
{
    public class Querier : Component // : ServiceComponent<IQuerier>, IQuerier
    {
        readonly Random _random = new Random();

        static Func<T, Action<bool>, bool> Do<T>(Func<T, bool> func) =>
            (e, continuation) =>
            {
                continuation(func(e));
                return true;
            };

        public Querier()
        {
            OnAsync(Do<QuerySwitchEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().GetSwitch(q.SwitchId) ? 1 : 0, q.Immediate)));
            OnAsync(Do<QueryTickerEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().GetTicker(q.TickerId), q.Immediate)));
            OnAsync(Do<QueryMapEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().MapId.Id, q.MapId.Id)));
            OnAsync(Do<QueryGoldEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().Party.TotalGold, q.Argument)));
            OnAsync(Do<QueryHasItemEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().Party.GetItemCount(q.ItemId), q.Immediate)));
            OnAsync(Do<QueryHasPartyMemberEvent>(q => Resolve<IGameState>().Party.StatusBarOrder.Any(x => x.Id == q.PartyMemberId)));
            OnAsync(Do<QueryLeaderEvent>(q => Resolve<IGameState>().Party.Leader.Id == q.PartyMemberId));
            OnAsync(Do<QueryRandomChanceEvent>(q => _random.Next(100) < q.Argument));
            OnAsync(Do<QueryTriggerTypeEvent>(q => Resolve<IEventManager>().Context.Source.Trigger == (TriggerTypes)q.Argument));
            OnAsync(Do<QueryChosenVerbEvent>(q => Resolve<IEventManager>().Context.Source.Trigger.HasFlag((TriggerTypes)(1 << (int)q.TriggerType))));
            OnAsync(Do<QueryUsedItemEvent>(q => Resolve<IEventManager>().Context.Source.AssetId == (AssetId)q.ItemId));
            OnAsync(Do<QueryPreviousActionResultEvent> (q => Resolve<IEventManager>().LastEventResult));
            OnAsync(Do<QueryScriptDebugModeEvent>(q => false));

            OnAsync(Do<QueryNpcActiveEvent>(q =>
            {
                Error("TODO: Query NpcActive");
                return true;
            }));
            OnAsync(Do<QueryConsciousEvent> (q =>
            {
                Error("TODO: Query Party member conscious");
                return true;
            }));
            OnAsync(Do<QueryEventUsedEvent> (q =>
            {
                Error("TODO: Query event already used");
                return false;
            }));
            OnAsync(Do<QueryDemoVersionEvent> (q =>
            {
                Error("TODO: Query is demo");
                return false;
            }));

            OnAsync<PromptPlayerEvent, bool>((q, continuation) =>
            {
                var context = Resolve<IEventManager>().Context;
                if (context.Source == null)
                    return false;

                return RaiseAsync(new YesNoPromptEvent(new StringId(q.TextSourceId, q.Argument)), continuation) > 0;
            });

            OnAsync<PromptPlayerNumericEvent, bool>((q, continuation) =>
           {
               var context = Resolve<IEventManager>().Context;
               if (context?.Source == null)
                   return false;

               return RaiseAsync(
                  new NumericPromptEvent((TextId)Base.SystemText.MsgBox_EnterNumber, 0, 9999),
                  x => continuation(x == q.Argument)) > 0;
           });
        }
/*
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
*/
    }
}
