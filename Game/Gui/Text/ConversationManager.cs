using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Text
{
    public class ConversationManager : Component
    {
        Conversation _conversation;

        public ConversationManager()
        {
            On<TextEvent>(OnTextEvent);
            On<MapTextEvent>(OnBaseTextEvent);
            On<EventTextEvent>(OnBaseTextEvent);
            On<NpcTextEvent>(OnNpcTextEvent);
            On<PartyMemberTextEvent>(OnPartyMemberTextEvent);
            On<StartDialogueEvent>(StartDialogue);
        }

        void OnNpcTextEvent(NpcTextEvent e)
        {
            e.Acknowledge();
            var state = Resolve<IGameState>();
            var sheet = state.GetNpc(e.NpcId);
            var eventManager = Resolve<IEventManager>();
            var mapManager = Resolve<IMapManager>();

            var (useEventText, textSourceId) = eventManager.Context?.Source switch
            {
                EventSource.Map map => (false, (int) map.MapId),
                EventSource.EventSet eventSet => (true, (int) eventSet.EventSetId),
                _ => (false, (int)mapManager.Current.MapId)
            };

            var textEvent = 
                useEventText
                    ? (BaseTextEvent)new EventTextEvent(
                        (EventSetId)textSourceId,
                        e.TextId,
                        TextLocation.TextInWindowWithPortrait,
                        sheet.PortraitId)
                    : new MapTextEvent(
                        (MapDataId) textSourceId,
                        e.TextId,
                        TextLocation.TextInWindowWithPortrait,
                        sheet.PortraitId);

            OnBaseTextEvent((BaseTextEvent)textEvent.CloneWithCallback(e.Complete));
        }

        void OnPartyMemberTextEvent(PartyMemberTextEvent e)
        {
            e.Acknowledge();
            var state = Resolve<IGameState>();
            var party = Resolve<IParty>();
            var sheet = state.GetPartyMember(e.MemberId ?? party.Leader);
            var eventManager = Resolve<IEventManager>();
            var mapManager = Resolve<IMapManager>();

            var (useEventText, textSourceId) = eventManager.Context?.Source switch
            {
                EventSource.Map map => (false, (int) map.MapId),
                EventSource.EventSet eventSet => (true, (int) eventSet.EventSetId),
                _ => (false, (int)mapManager.Current.MapId)
            };

            var textEvent = 
                useEventText
                    ?  (BaseTextEvent)new EventTextEvent(
                        (EventSetId)textSourceId,
                        e.TextId,
                        TextLocation.TextInWindowWithPortrait,
                        sheet.PortraitId)
                    : new MapTextEvent(
                        (MapDataId) textSourceId,
                        e.TextId,
                        TextLocation.TextInWindowWithPortrait,
                        sheet.PortraitId);

            OnBaseTextEvent((BaseTextEvent)textEvent.CloneWithCallback(e.Complete));
        }

        void OnTextEvent(TextEvent e)
        {
            e.Acknowledge();
            var eventManager = Resolve<IEventManager>();
            var mapManager = Resolve<IMapManager>();

            var (useEventText, textSourceId) = eventManager.Context?.Source switch
            {
                EventSource.Map map => (false, (int) map.MapId),
                EventSource.EventSet eventSet => (true, (int) eventSet.EventSetId),
                _ => (false, (int)mapManager.Current.MapId)
            };

            var textEvent = 
                useEventText
                    ?  (BaseTextEvent)new EventTextEvent(
                        (EventSetId)textSourceId,
                        e.TextId,
                        e.Location,
                        e.PortraitId)
                    : new MapTextEvent(
                        (MapDataId) textSourceId,
                        e.TextId,
                        e.Location,
                        e.PortraitId);

            OnBaseTextEvent((BaseTextEvent)textEvent.CloneWithCallback(e.Complete));
        }

        void OnBaseTextEvent(BaseTextEvent textEvent)
        {
            if (_conversation?.OnText(textEvent) == true)
                return;

            var textManager = Resolve<ITextManager>();
            switch(textEvent.Location)
            {
                case null:
                case TextLocation.TextInWindow:
                {
                    textEvent.Acknowledge();
                    var dialog = AttachChild(new TextDialog(textManager.FormatTextEvent(textEvent, FontColor.White)));
                    dialog.Closed += (sender, _) => textEvent.Complete();
                    break;
                }

                case TextLocation.TextInWindowWithPortrait:
                case TextLocation.TextInWindowWithPortrait2:
                case TextLocation.TextInWindowWithPortrait3:
                {
                    textEvent.Acknowledge();
                    SmallPortraitId portraitId = textEvent.PortraitId ?? Resolve<IParty>().Leader.ToPortraitId();

                    if (textEvent.Location == TextLocation.TextInWindowWithPortrait2) // TODO: ??? work out how this is meant to work.
                        portraitId = SmallPortraitId.Rainer;

                    var dialog = AttachChild(new TextDialog(textManager.FormatTextEvent(textEvent, FontColor.Yellow), portraitId));
                    dialog.Closed += (sender, _) => textEvent.Complete();
                    break;
                }

                case TextLocation.QuickInfo:
                    Raise(new DescriptionTextExEvent(textManager.FormatTextEvent(textEvent, FontColor.White)));
                    textEvent.Complete();
                    break;

                case TextLocation.Conversation:
                case TextLocation.ConversationQuery:
                case TextLocation.ConversationOptions:
                case TextLocation.StandardOptions:
                    break; // Handled by Conversation

                default:
                    Raise(new DescriptionTextExEvent(textManager.FormatTextEvent(textEvent, FontColor.White))); // TODO:
                    textEvent.Complete();
                    break;
            }
        }

        void StartDialogue(StartDialogueEvent e)
        {
            e.Acknowledge();
            var party = Resolve<IParty>();
            var assets = Resolve<IAssetManager>();
            var npc = assets.LoadCharacter(e.NpcId);
            _conversation = AttachChild(new Conversation(party?.Leader ?? PartyCharacterId.Tom, npc));

            _conversation.Complete += (sender, args) =>
            {
                e.Complete();
                Children.Remove(_conversation);
                _conversation.Detach();
                _conversation = null;
            };
        }
    }
}