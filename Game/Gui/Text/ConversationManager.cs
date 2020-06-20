using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.State;
using UAlbion.Game.Text;

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
            On<StartPartyDialogueEvent>(StartPartyDialogue);
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

            textEvent.SetCallback(e.Complete);
            OnBaseTextEvent(textEvent);
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

            textEvent.SetCallback(e.Complete);
            OnBaseTextEvent(textEvent);
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

            textEvent.SetCallback(e.Complete);
            OnBaseTextEvent(textEvent);
        }

        void OnBaseTextEvent(BaseTextEvent textEvent)
        {
            if (_conversation?.OnText(textEvent) == true)
                return;

            var tf = Resolve<ITextFormatter>();
            switch(textEvent.Location)
            {
                case null:
                case TextLocation.TextInWindow:
                {
                    textEvent.Acknowledge();
                    var dialog = AttachChild(new TextDialog(tf.Format(textEvent.ToId())));
                    dialog.Closed += (sender, _) =>
                    {
                        textEvent.Complete();
                        RemoveChild(dialog);
                    };
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

                    var text = tf.Ink(FontColor.Yellow).Format(textEvent.ToId());
                    var dialog = AttachChild(new TextDialog(text, portraitId));
                    dialog.Closed += (sender, _) =>
                    {
                        textEvent.Complete();
                        RemoveChild(dialog);
                    };
                    break;
                }

                case TextLocation.QuickInfo:
                    Raise(new DescriptionTextEvent(tf.Format(textEvent.ToId())));
                    textEvent.Complete();
                    break;

                case TextLocation.Conversation:
                case TextLocation.ConversationQuery:
                case TextLocation.ConversationOptions:
                case TextLocation.StandardOptions:
                    break; // Handled by Conversation

                default:
                    Raise(new DescriptionTextEvent(tf.Format(textEvent.ToId()))); // TODO:
                    textEvent.Complete();
                    break;
            }
        }

        void StartDialogue(StartDialogueEvent e)
        {
            e.Acknowledge();
            var party = Resolve<IParty>();
            var assets = Resolve<IAssetManager>();
            var npc = assets.LoadNpc(e.NpcId);
            _conversation = AttachChild(new Conversation(party?.Leader ?? PartyCharacterId.Tom, npc));

            _conversation.Complete += (sender, args) =>
            {
                e.Complete();
                _conversation.Remove();
                _conversation = null;
            };
        }

        void StartPartyDialogue(StartPartyDialogueEvent e)
        {
            e.Acknowledge();
            var assets = Resolve<IAssetManager>();
            var npc = assets.LoadPartyMember(e.MemberId);
            _conversation = AttachChild(new Conversation(PartyCharacterId.Tom, npc));

            _conversation.Complete += (sender, args) =>
            {
                e.Complete();
                _conversation.Remove();
                _conversation = null;
            };
        }
    }
}