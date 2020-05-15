using System;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Map;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game
{
    public class Querier : ServiceComponent<IQuerier>, IQuerier
    {
        readonly Random _random = new Random();

        public Querier()
        {
            On<QueryVerbEvent>(Query);
            On<QueryItemEvent>(Query);
            On<PromptPlayerEvent>(Query);
            On<PromptPlayerNumericEvent>(Query);
            On<QueryEvent>(Query);
        }

        public void Query(IQueryEvent query)
        {
            var context = Resolve<IEventManager>().Context;
            var result = InnerQuery(context, query, false);
            if (result.HasValue && context != null)
                context.LastEventResult = result.Value;
        }

        public bool? QueryDebug(IQueryEvent query)
        {
            var context = Resolve<IEventManager>().Context;
            return InnerQuery(context, query, true);
        }

        bool? InnerQuery(EventContext context, IQueryEvent query, bool debugInspect)
        {
            var game = Resolve<IGameState>();
            switch (query, query.QueryType)
            {
                case (QueryEvent q, QueryType.TemporarySwitch):      return Compare(q.Operation, game.GetSwitch(q.Argument) ? 1 : 0, q.Immediate);
                case (QueryEvent q, QueryType.Ticker):               return Compare(q.Operation, game.GetTicker(q.Argument), q.Immediate);
                case (QueryEvent q, QueryType.CurrentMapId):         return Compare(q.Operation, (int)game.MapId, q.Immediate);
                case (QueryEvent q, QueryType.HasEnoughGold):        return Compare(q.Operation, game.Party.TotalGold, q.Argument);
                case (QueryItemEvent q, QueryType.InventoryHasItem): return Compare(q.Operation, game.Party.GetItemCount(q.ItemId), q.Immediate);
                case (QueryEvent q, QueryType.HasPartyMember):       return game.Party.StatusBarOrder.Any(x => (int)x.Id == q.Argument);
                case (QueryEvent q, QueryType.IsPartyMemberLeader):  return (int)game.Party.Leader == q.Argument;
                case (QueryEvent q, QueryType.RandomChance):         return _random.Next(100) < q.Argument;
                case (QueryEvent q, QueryType.TriggerType):          return (ushort)context.Source.Trigger == q.Argument;
                case (QueryVerbEvent verb, QueryType.ChosenVerb):    return context.Source.Trigger.HasFlag((TriggerType)(1 << (int)verb.Verb));
                case (QueryItemEvent q, QueryType.UsedItemId):       return context.Source is EventSource.Item item && item.ItemId == q.ItemId;
                case (_, QueryType.PreviousActionResult):            return context.LastEventResult;

                case (_, QueryType.IsScriptDebugModeActive): { return false; }
                case (_, QueryType.IsNpcActive):             { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query NpcActive")); break; }
                case (_, QueryType.IsPartyMemberConscious):  { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query Party member conscious")); break; }
                case (_, QueryType.EventAlreadyUsed):        { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query event already used")); return false; }
                case (_, QueryType.IsDemoVersion):           { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query is demo")); return false; }

                case (PromptPlayerEvent prompt, QueryType.PromptPlayer):
                {
                    if (context.Source == null || debugInspect)
                        break;

                    var assets = Resolve<IAssetManager>();
                    var stringId = new StringId(prompt.TextType, prompt.TextSourceId, prompt.TextId);

                    var dialog = AttachChild(new PromptDialog(
                        new DynamicText(() =>
                        {
                            var settings = Resolve<ISettings>();
                            var template = assets.LoadString(stringId, settings.Gameplay.Language);
                            return new TextFormatter(assets, settings.Gameplay.Language)
                                .Format(template).Blocks;
                        })));

                    prompt.Acknowledge();
                    dialog.Closed += (sender, _) => prompt.Complete();
                    return null;
                }

                case (PromptPlayerNumericEvent prompt, QueryType.PromptPlayerNumeric):
                {
                    if (context?.Source == null || debugInspect)
                        break;

                    var assets = Resolve<IAssetManager>();

                    var dialog = AttachChild(new NumericPromptDialog(
                        new DynamicText(() =>
                        {
                            var settings = Resolve<ISettings>();
                            var template = assets.LoadString(SystemTextId.MsgBox_EnterNumber, settings.Gameplay.Language);
                            return new TextFormatter(assets, settings.Gameplay.Language)
                                .Format(template).Blocks;
                        }), 0, 9999));

                    prompt.Acknowledge();
                    dialog.Closed += (sender, _) =>
                    {
                        context.LastEventResult = prompt.Argument == dialog.Value;
                        prompt.Complete();
                    };
                    return null;
                }

                default:
                    Raise(new LogEvent(LogEvent.Level.Error, $"Unhandled query event {query} (subtype {query.QueryType}"));
                    break;
            }

            return true;
        }

        bool Compare(QueryOperation operation, int value, int immediate) =>
            operation switch
            {
                QueryOperation.IsTrue => (value != 0),
                QueryOperation.NotEqual => (value != immediate),
                QueryOperation.OpUnk2 => (value == immediate),
                QueryOperation.Equals => (value == immediate),
                QueryOperation.OpUnk3 => (value == immediate),
                QueryOperation.GreaterThan => (value > immediate),
                QueryOperation.OpUnk5 => (value == immediate),
                _ => true
            };
    }
}
