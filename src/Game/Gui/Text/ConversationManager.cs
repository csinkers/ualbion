using System;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Text;

public class ConversationManager : ServiceComponent<IConversationManager>, IConversationManager
{
    public Conversation Conversation { get; private set; }

    public ConversationManager()
    {
        OnAsync<StartDialogueEvent>(StartDialogue);
        OnAsync<StartPartyDialogueEvent>(StartPartyDialogue);
        OnAsync<TextEvent>(OnBaseTextEvent);
        OnAsync<NpcTextEvent>(OnNpcTextEvent);
        OnAsync<PartyMemberTextEvent>(OnPartyMemberTextEvent);
    }

    StringSetId ContextTextSource =>
        Context is EventContext { EventSet: { } } context
            ? context.EventSet.StringSetId
            : Resolve<IMapManager>().Current.MapId.ToMapText();

    bool OnNpcTextEvent(NpcTextEvent e, Action continuation)
    {
        var textEvent = new TextEvent(e.TextId, TextLocation.PortraitLeft2, e.NpcId);
        return OnBaseTextEvent(textEvent, continuation);
    }

    bool OnPartyMemberTextEvent(PartyMemberTextEvent e, Action continuation)
    {
        var textEvent = new TextEvent(e.TextId, TextLocation.PortraitLeft, e.MemberId.ToSheet());
        return OnBaseTextEvent(textEvent, continuation);
    }

    bool OnBaseTextEvent(TextEvent mapTextEvent, Action continuation)
    {
        var conversationResult = Conversation?.OnText(mapTextEvent, continuation);
        if (conversationResult.HasValue)
            return conversationResult.Value;

        var tf = Resolve<ITextFormatter>();
        switch (mapTextEvent.Location)
        {
            case TextLocation.NoPortrait:
                {
                    var dialog = AttachChild(new TextDialog(tf.Format(mapTextEvent.ToId(ContextTextSource))));
                    dialog.Closed += (_, _) => continuation();
                    return true;
                }

            case TextLocation.PortraitLeft:
            case TextLocation.PortraitLeft2:
            case TextLocation.PortraitLeft3:
                {
                    var portraitId = GetPortrait(mapTextEvent.Speaker);
                    var text = tf.Ink(Base.Ink.Yellow).Format(mapTextEvent.ToId(ContextTextSource));
                    var dialog = AttachChild(new TextDialog(text, portraitId));
                    dialog.Closed += (_, _) => continuation();
                    return true;
                }

            case TextLocation.QuickInfo:
                Raise(new DescriptionTextEvent(tf.Format(mapTextEvent.ToId(ContextTextSource))));
                continuation();
                return true;

            case TextLocation.Conversation:
            case TextLocation.ConversationQuery:
            case TextLocation.ConversationOptions:
            case TextLocation.StandardOptions:
                break; // Handled by Conversation

            default:
                Raise(new DescriptionTextEvent(tf.Format(mapTextEvent.ToId(ContextTextSource)))); // TODO:
                continuation();
                return true;
        }

        return false;
    }

    SpriteId GetPortrait(SheetId id)
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
        if (npc == null)
        {
            ApiUtil.Assert($"Could not load NPC {e.NpcId}");
            return false;
        }

        var wasRunning = Resolve<IClock>().IsRunning;
        if (wasRunning)
            Raise(new StopClockEvent());

        Conversation = AttachChild(new Conversation(party?.Leader.Id ?? Base.PartyMember.Tom, npc));
        Conversation.Complete += (_, _) =>
        {
            Conversation.Remove();
            Conversation = null;

            if (wasRunning)
                Raise(new StartClockEvent());

            continuation();
        };
        Conversation.StartDialogue();
        return true;
    }

    bool StartPartyDialogue(StartPartyDialogueEvent e, Action continuation)
    {
        var assets = Resolve<IAssetManager>();
        var sheet = assets.LoadSheet(e.MemberId.ToSheet());
        if (sheet == null)
        {
            Error($"Could not load NPC info for \"{e.MemberId}\"");
            continuation();
            return true;
        }

        Conversation = AttachChild(new Conversation(Base.PartyMember.Tom, sheet));
        Conversation.Complete += (_, _) => { Conversation = null; continuation(); };
        Conversation.StartDialogue();
        return true;
    }
}
