using System;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Text;

public class ConversationManager : Component
{
    Conversation _conversation;

    public ConversationManager()
    {
        OnAsync<ScriptTextEvent>(OnTextEvent);
        OnAsync<MapTextEvent>(OnBaseTextEvent);
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
        var textEvent = new MapTextEvent(ContextTextSource, e.TextId, TextLocation.PortraitLeft2, e.NpcId);
        return OnBaseTextEvent(textEvent, continuation);
    }

    bool OnPartyMemberTextEvent(PartyMemberTextEvent e, Action continuation)
    {
        var textEvent = new MapTextEvent(ContextTextSource, e.TextId, TextLocation.PortraitLeft, e.MemberId);
        return OnBaseTextEvent(textEvent, continuation);
    }

    bool OnTextEvent(ScriptTextEvent e, Action continuation)
    {
        var textEvent = new MapTextEvent(ContextTextSource, e.TextId, e.Location, e.Speaker);
        return OnBaseTextEvent(textEvent, continuation);
    }

    bool OnBaseTextEvent(MapTextEvent mapTextEvent, Action continuation)
    {
        var conversationResult = _conversation?.OnText(mapTextEvent, continuation);
        if (conversationResult.HasValue)
            return conversationResult.Value;

        var tf = Resolve<ITextFormatter>();
        switch(mapTextEvent.Location)
        {
            case TextLocation.NoPortrait:
                {
                    var dialog = AttachChild(new TextDialog(tf.Format(mapTextEvent.ToId())));
                    dialog.Closed += (_, _) => continuation();
                    return true;
                }

            case TextLocation.PortraitLeft:
            case TextLocation.PortraitLeft2:
            case TextLocation.PortraitLeft3:
                {
                    var portraitId = GetPortrait(mapTextEvent.Speaker);
                    var text = tf.Ink(FontColor.Yellow).Format(mapTextEvent.ToId());
                    var dialog = AttachChild(new TextDialog(text, portraitId));
                    dialog.Closed += (_, _) => continuation();
                    return true;
                }

            case TextLocation.QuickInfo:
                Raise(new DescriptionTextEvent(tf.Format(mapTextEvent.ToId())));
                continuation();
                return true;

            case TextLocation.Conversation:
            case TextLocation.ConversationQuery:
            case TextLocation.ConversationOptions:
            case TextLocation.StandardOptions:
                break; // Handled by Conversation

            default:
                Raise(new DescriptionTextEvent(tf.Format(mapTextEvent.ToId()))); // TODO:
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
        if (sheet is { PortraitId: { IsNone: false } })
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
        _conversation.Complete += (_, _) => { _conversation = null; continuation(); };
        _conversation.StartDialogue();
        return true;
    }

    bool StartPartyDialogue(StartPartyDialogueEvent e, Action continuation)
    {
        var assets = Resolve<IAssetManager>();
        var npc = assets.LoadSheet(e.MemberId);
        if (npc == null)
        {
            Error($"Could not load NPC info for \"{e.MemberId}\"");
            continuation();
            return true;
        }

        _conversation = AttachChild(new Conversation(Base.PartyMember.Tom, npc));
        _conversation.Complete += (_, _) => { _conversation = null; continuation(); };
        _conversation.StartDialogue();
        return true;
    }
}