using System;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game;

public class Querier : Component // : ServiceComponent<IQuerier>, IQuerier
{
    static AsyncMethod<T, bool> Do<T>(Func<T, bool> func) where T : IAsyncEvent<bool> =>
        (e, continuation) =>
        {
            continuation(func(e));
            return true;
        };

    public Querier()
    {
        OnAsync(Do<QueryChosenVerbEvent>(q =>
            {
                var triggers = ((EventContext)Context).Source.Trigger;
                var queryTrigger = (TriggerTypes)(1 << (int)q.TriggerType);
                return (triggers & queryTrigger) != 0;
            }));

        OnAsync(          Do<QueryGoldEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().Party.TotalGold, q.Argument)));
        OnAsync(       Do<QueryHasItemEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().Party.GetItemCount(q.ItemId), q.Immediate)));
        OnAsync(Do<QueryHasPartyMemberEvent>(q => Resolve<IGameState>().Party.StatusBarOrder.Any(x => x.Id == q.PartyMemberId)));
        OnAsync(          Do<QueryHourEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().Time.Hour, q.Argument)));
        OnAsync(        Do<QueryLeaderEvent>(q => Resolve<IGameState>().Party.Leader.Id == q.PartyMemberId));
        OnAsync(           Do<QueryMapEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().MapId.Id, q.MapId.Id)));
        OnAsync(Do<QueryPreviousActionResultEvent> (_ => ((EventContext)Context).LastEventResult));
        OnAsync(   Do<QueryRandomChanceEvent>(q => Resolve<IRandom>().Generate(100) < q.Argument));
        OnAsync(         Do<QuerySwitchEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().GetSwitch(q.SwitchId) ? 1 : 0, q.Immediate)));
        OnAsync(         Do<QueryTickerEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().GetTicker(q.TickerId), q.Immediate)));
        OnAsync(    Do<QueryTriggerTypeEvent>(q => ((EventContext)Context).Source.Trigger == (TriggerTypes)q.Argument));
        OnAsync(       Do<QueryUsedItemEvent>(q => ((EventContext)Context).Source.AssetId == (AssetId)q.ItemId));
        OnAsync(      Do<QueryNpcActiveEvent>(q => Resolve<IGameState>().IsNpcDisabled(MapId.None, q.Immediate)));
        OnAsync(Do<QueryScriptDebugModeEvent>(_ => false));

        OnAsync(Do<QueryConsciousEvent> (q =>
        {
            var state = Resolve<IGameState>();
            var member = state.GetSheet(q.PartyMemberId.ToSheet());
            if (member == null)
                return false;
            return (member.Combat.Conditions & PlayerConditions.UnconsciousMask) == 0;
        }));

        OnAsync(Do<QueryEventUsedEvent> (_ =>
        {
            var context = (EventContext)Context;
            if (context.EventSet.Id.Type != AssetType.EventSet)
                return false;

            var game = Resolve<IGameState>();
            return game.IsEventUsed(context.EventSet.Id, context.LastAction);
        }));

        OnAsync(Do<QueryDemoVersionEvent> (_ =>
        {
            Error("TODO: Query is demo");
            return false;
        }));

        OnAsync<PromptPlayerEvent, bool>((q, continuation) =>
        {
            var context = (EventContext)Context;
            if (context.Source == null)
                return false;

            return RaiseAsync(new YesNoPromptEvent(new StringId(context.EventSet.TextId, q.Argument)), continuation) > 0;
        });

        OnAsync<PromptPlayerNumericEvent, bool>((q, continuation) =>
        {
            var context = (EventContext)Context;
            if (context?.Source == null)
                return false;

            return RaiseAsync(
                new NumericPromptEvent((TextId)Base.SystemText.MsgBox_EnterNumber, 0, 9999),
                x => continuation(x == q.Argument)) > 0;
        });

        OnAsync(Do<QueryChainActiveEvent>(q => !Resolve<IGameState>().IsChainDisabled(q.MapId, q.ChainNum)));
        OnAsync(Do<QueryNpcActiveOnMapEvent>(q => !Resolve<IGameState>().IsNpcDisabled(q.MapId, q.NpcNum)));
        OnAsync(Do<QueryNpcXEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().Npcs[q.Immediate].X, q.Argument)));
        OnAsync(Do<QueryNpcYEvent>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().Npcs[q.Immediate].Y, q.Argument)));

        // TODO
        OnAsync(Do<QueryUnkCEvent>(_ => false));
        OnAsync(Do<QueryUnk19Event>(_ => false));
        OnAsync(Do<QueryUnk1EEvent>(_ => false));
        OnAsync(Do<QueryUnk21Event>(_ => false));
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
