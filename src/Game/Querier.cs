using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game;

public class Querier : Component // : ServiceComponent<IQuerier>, IQuerier
{
#pragma warning disable CA1506 // '.ctor' is coupled with '66' different types from '15' different namespaces. Rewrite or refactor the code to decrease its class coupling below '41'.
    public Querier()
    {
        OnQuery<          QueryVerbEvent, bool>(q => ((EventContext)Context).Source.Trigger == q.TriggerType);
        OnQuery<          QueryGoldEvent, bool>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().Party.TotalGold, q.Argument));
        OnQuery<       QueryHasItemEvent, bool>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().Party.GetItemCount(q.ItemId), q.Immediate));
        OnQuery<QueryHasPartyMemberEvent, bool>(q => Resolve<IGameState>().Party.StatusBarOrder.Any(x => x.Id == q.PartyMemberId));
        OnQuery<          QueryHourEvent, bool>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().Time.Hour, q.Argument));
        OnQuery<        QueryLeaderEvent, bool>(q => Resolve<IGameState>().Party.Leader.Id == q.PartyMemberId);
        OnQuery<           QueryMapEvent, bool>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().MapId.Id, q.MapId.Id));
        OnQuery<QueryPreviousActionResultEvent, bool> (_ => ((EventContext)Context).LastEventResult);
        OnQuery<   QueryRandomChanceEvent, bool>(q => Resolve<IRandom>().Generate(100) < q.Argument);
        OnQuery<         QuerySwitchEvent, bool>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().GetSwitch(q.SwitchId) ? 1 : 0, q.Immediate));
        OnQuery<         QueryTickerEvent, bool>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().GetTicker(q.TickerId), q.Immediate));
        OnQuery<    QueryTriggerTypeEvent, bool>(q => ((EventContext)Context).Source.Trigger == (TriggerType)q.Argument);
        OnQuery<       QueryUsedItemEvent, bool>(q => ((EventContext)Context).Source.AssetId == (AssetId)q.ItemId);
        OnQuery<      QueryNpcActiveEvent, bool>(q => Resolve<IGameState>().IsNpcDisabled(MapId.None, q.Immediate));
        OnQuery<QueryScriptDebugModeEvent, bool>(_ => false);

        OnQuery<QueryConsciousEvent, bool>(q =>
        {
            var state = Resolve<IGameState>();
            var member = state.GetSheet(q.PartyMemberId.ToSheet());
            if (member == null)
                return false;
            return (member.Combat.Conditions & PlayerConditions.UnconsciousMask) == 0;
        });

        OnQuery<QueryEventUsedEvent, bool>(_ =>
        {
            var context = (EventContext)Context;
            if (context.EventSet.Id.Type != AssetType.EventSet)
                return false;

            var game = Resolve<IGameState>();
            return game.IsEventUsed(context.EventSet.Id, context.LastAction);
        });

        OnQuery<QueryDemoVersionEvent, bool>(_ =>
        {
            Error("TODO: Query is demo");
            return false;
        });

        OnQueryAsync<PromptPlayerEvent, bool>(async q =>
        {
            var context = (EventContext)Context;
            if (context.Source == null)
                return false;

            var innerEvent = new YesNoPromptEvent(new StringId(context.EventSet.StringSetId, q.Argument));
            return await RaiseQueryA(innerEvent);
        });

        OnQueryAsync<PromptPlayerNumericEvent, bool>(async q =>
        {
            var context = (EventContext)Context;
            if (context?.Source == null)
                return false;

            var innerEvent = new NumericPromptEvent(Base.SystemText.MsgBox_EnterNumber, 0, 9999);
            var result = await RaiseQueryA(innerEvent);
            return result == q.Argument;
        });

        OnQuery<QueryChainActiveEvent, bool>(q => !Resolve<IGameState>().IsChainDisabled(q.MapId, q.ChainNum));
        OnQuery<QueryNpcActiveOnMapEvent, bool>(q => !Resolve<IGameState>().IsNpcDisabled(q.MapId, q.NpcNum));
        OnQuery<QueryNpcXEvent, bool>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().Npcs[q.Immediate].X, q.Argument));
        OnQuery<QueryNpcYEvent, bool>(q => FormatUtil.Compare(q.Operation, Resolve<IGameState>().Npcs[q.Immediate].Y, q.Argument));

        // TODO
        OnQuery<QueryUnkCEvent, bool>(_ => false);
        OnQuery<QueryUnk19Event, bool>(_ => false);
        OnQuery<QueryUnk1EEvent, bool>(_ => false);
        OnQuery<QueryUnk21Event, bool>(_ => false);
    }
#pragma warning restore CA1506 // '.ctor' is coupled with '66' different types from '15' different namespaces. Rewrite or refactor the code to decrease its class coupling below '41'.

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
