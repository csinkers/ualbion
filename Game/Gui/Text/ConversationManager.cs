using System;
using UAlbion.Api;
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
            OnAsync<TextEvent>(OnTextEvent);
            OnAsync<MapTextEvent>(OnBaseTextEvent);
            OnAsync<EventTextEvent>(OnBaseTextEvent);
            OnAsync<NpcTextEvent>(OnNpcTextEvent);
            OnAsync<PartyMemberTextEvent>(OnPartyMemberTextEvent);
            OnAsync<StartDialogueEvent>(StartDialogue);
            OnAsync<StartPartyDialogueEvent>(StartPartyDialogue);
        }

        bool OnNpcTextEvent(NpcTextEvent e, Action continuation)
        {
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

            return OnBaseTextEvent(textEvent, continuation);
        }

        bool OnPartyMemberTextEvent(PartyMemberTextEvent e, Action continuation)
        {
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

            return OnBaseTextEvent(textEvent, continuation);
        }

        bool OnTextEvent(TextEvent e, Action continuation)
        {
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

            return OnBaseTextEvent(textEvent, continuation);
        }

        bool OnBaseTextEvent(BaseTextEvent textEvent, Action continuation)
        {
            var conversationResult = _conversation?.OnText(textEvent, continuation);
            if (conversationResult.HasValue)
                return conversationResult.Value;

            var tf = Resolve<ITextFormatter>();
            switch(textEvent.Location)
            {
                case null:
                case TextLocation.TextInWindow:
                {
                    var dialog = AttachChild(new TextDialog(tf.Format(textEvent.ToId())));
                    dialog.Closed += (sender, _) => continuation();
                    return true;
                }

                case TextLocation.TextInWindowWithPortrait:
                case TextLocation.TextInWindowWithPortrait2:
                case TextLocation.TextInWindowWithPortrait3:
                {
                    SmallPortraitId portraitId = textEvent.PortraitId ?? Resolve<IParty>().Leader.ToPortraitId();

                    if (textEvent.Location == TextLocation.TextInWindowWithPortrait2) // TODO: ??? work out how this is meant to work.
                        portraitId = SmallPortraitId.Rainer;

                    var text = tf.Ink(FontColor.Yellow).Format(textEvent.ToId());
                    var dialog = AttachChild(new TextDialog(text, portraitId));
                    dialog.Closed += (sender, _) => continuation();
                    return true;
                }

                case TextLocation.QuickInfo:
                    Raise(new DescriptionTextEvent(tf.Format(textEvent.ToId())));
                    continuation();
                    return true;

                case TextLocation.Conversation:
                case TextLocation.ConversationQuery:
                case TextLocation.ConversationOptions:
                case TextLocation.StandardOptions:
                    break; // Handled by Conversation

                default:
                    Raise(new DescriptionTextEvent(tf.Format(textEvent.ToId()))); // TODO:
                    continuation();
                    return true;
            }

            return false;
        }

        bool StartDialogue(StartDialogueEvent e, Action continuation)
        {
            var party = Resolve<IParty>();
            var assets = Resolve<IAssetManager>();
            var npc = assets.LoadNpc(e.NpcId);
            _conversation = AttachChild(new Conversation(party?.Leader ?? PartyCharacterId.Tom, npc));

            _conversation.Complete += (sender, args) =>
            {
                _conversation.Remove();
                _conversation = null;
                continuation();
            };
            return true;
        }

        bool StartPartyDialogue(StartPartyDialogueEvent e, Action continuation)
        {
            var assets = Resolve<IAssetManager>();
            var npc = assets.LoadPartyMember(e.MemberId);
            if (npc == null)
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Could not load NPC info for \"{e.MemberId}\""));
                continuation();
                return true;
            }

            _conversation = AttachChild(new Conversation(PartyCharacterId.Tom, npc));
            _conversation.Complete += (sender, args) => { _conversation = null; continuation(); };
            return true;
        }
    }
}