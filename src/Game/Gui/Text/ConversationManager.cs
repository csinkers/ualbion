using System;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.Assets;
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
            OnAsync<ContextTextEvent>(OnTextEvent);
            OnAsync<TextEvent>(OnBaseTextEvent);
            OnAsync<NpcTextEvent>(OnNpcTextEvent);
            OnAsync<PartyMemberTextEvent>(OnPartyMemberTextEvent);
            OnAsync<StartDialogueEvent>(StartDialogue);
            OnAsync<StartPartyDialogueEvent>(StartPartyDialogue);
        }

        TextId ContextTextSource =>
            Resolve<IEventManager>().Context?.Source.TextSource 
            ?? 
            Resolve<IMapManager>().Current.MapId.ToMapText();

        bool OnNpcTextEvent(NpcTextEvent e, Action continuation)
        {
            var textEvent = new TextEvent(ContextTextSource, e.TextId, TextLocation.PortraitLeft, e.NpcId);
            return OnBaseTextEvent(textEvent, continuation);
        }

        bool OnPartyMemberTextEvent(PartyMemberTextEvent e, Action continuation)
        {
            var party = Resolve<IParty>();
            var textEvent = new TextEvent(ContextTextSource, e.TextId, TextLocation.PortraitLeft, e.MemberId);
            return OnBaseTextEvent(textEvent, continuation);
        }

        bool OnTextEvent(ContextTextEvent e, Action continuation)
        {
            var textEvent = new TextEvent(ContextTextSource, e.TextId, e.Location, e.NpcId);
            return OnBaseTextEvent(textEvent, continuation);
        }

        bool OnBaseTextEvent(TextEvent textEvent, Action continuation)
        {
            var conversationResult = _conversation?.OnText(textEvent, continuation);
            if (conversationResult.HasValue)
                return conversationResult.Value;

            var tf = Resolve<ITextFormatter>();
            switch(textEvent.Location)
            {
                case null:
                case TextLocation.NoPortrait:
                {
                    var dialog = AttachChild(new TextDialog(tf.Format(textEvent.ToId())));
                    dialog.Closed += (sender, _) => continuation();
                    return true;
                }

                case TextLocation.PortraitLeft:
                case TextLocation.PortraitLeft2:
                case TextLocation.PortraitLeft3:
                {
                    var portraitId = GetPortrait(textEvent.CharacterId);
                    if (textEvent.Location == TextLocation.PortraitLeft2) // TODO: ??? work out how this is meant to work.
                        portraitId = Base.Portrait.Rainer;
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

        SpriteId GetPortrait(CharacterId id)
        {
            if (id.IsNone)
                return Base.Portrait.GibtEsNicht;

            var sheet = Resolve<IGameState>().GetSheet(id);
            if (sheet != null && !sheet.PortraitId.IsNone)
                return sheet.PortraitId;
            var leader = Resolve<IParty>().Leader.Effective;
            return leader.PortraitId;
        }

        bool StartDialogue(StartDialogueEvent e, Action continuation)
        {
            var party = Resolve<IParty>();
            var assets = Resolve<IAssetManager>();
            var npc = assets.LoadSheet(e.NpcId);
            if(npc == null)
            {
                ApiUtil.Assert($"Could not load NPC {e.NpcId}");
                return false;
            }

            _conversation = AttachChild(new Conversation(party?.Leader.Id ?? Base.PartyMember.Tom, npc));
            _conversation.Complete += (sender, args) => { _conversation = null; continuation(); };
            _conversation.StartDialogue();
            return true;
        }

        bool StartPartyDialogue(StartPartyDialogueEvent e, Action continuation)
        {
            var assets = Resolve<IAssetManager>();
            var npc = assets.LoadSheet(e.MemberId);
            if (npc == null)
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Could not load NPC info for \"{e.MemberId}\""));
                continuation();
                return true;
            }

            _conversation = AttachChild(new Conversation(Base.PartyMember.Tom, npc));
            _conversation.Complete += (sender, args) => { _conversation = null; continuation(); };
            _conversation.StartDialogue();
            return true;
        }
    }
}
