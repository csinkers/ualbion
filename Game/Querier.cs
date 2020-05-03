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
            On<QueryVerbEvent>(e =>
            {
                var result = Query(e.Context, e);
                if (result.HasValue)
                    e.Context.LastEventResult = result.Value;
            });
            On<QueryItemEvent>(e =>
            {
                var result = Query(e.Context, e);
                if (result.HasValue)
                    e.Context.LastEventResult = result.Value;
            });
            On<PromptPlayerEvent>(e =>
            {
                var result = Query(e.Context, e);
                if (result.HasValue)
                    e.Context.LastEventResult = result.Value;
            });
            On<PromptPlayerNumericEvent>(e =>
            {
                var result = Query(e.Context, e);
                if (result.HasValue)
                    e.Context.LastEventResult = result.Value;
            });
            On<QueryEvent>(e =>
            {
                var result = Query(e.Context, e);
                if (result.HasValue)
                    e.Context.LastEventResult = result.Value;
            });
        }
        public bool? Query(EventContext context, IQueryEvent query, bool debugInspect = false)
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
                case (_, QueryType.EventAlreadyUsed):        { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query event already used")); break; }
                case (_, QueryType.IsDemoVersion):           { Raise(new LogEvent(LogEvent.Level.Error, "TODO: Query is demo")); return false; }

                case (PromptPlayerEvent prompt, QueryType.PromptPlayer):
                {
                    if (prompt.Context?.Source == null || debugInspect)
                        break;

                    var assets = Resolve<IAssetManager>();

                    var stringId = new StringId(
                        prompt.Context.Source.Type,
                        prompt.Context.Source.Id,
                        prompt.TextId);

                    var dialog = AttachChild(new PromptDialog(
                        new DynamicText(() =>
                        {
                            var settings = Resolve<ISettings>();
                            var template = assets.LoadString(stringId, settings.Gameplay.Language);
                            return new TextFormatter(assets, settings.Gameplay.Language)
                                .Format(template).Blocks;
                        })));

                    prompt.Acknowledged = true;
                    dialog.Closed += (sender, _) => prompt.Complete();
                    return null;
                }

                case (PromptPlayerNumericEvent prompt, QueryType.PromptPlayerNumeric):
                {
                    if (prompt.Context?.Source == null || debugInspect)
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

                    prompt.Acknowledged = true;
                    dialog.Closed += (sender, _) =>
                    {
                        prompt.Context.LastEventResult = prompt.Argument == dialog.Value;
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
