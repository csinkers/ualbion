using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Text;

public class ConversationManager : GameServiceComponent<IConversationManager>, IConversationManager
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

    AlbionTask OnNpcTextEvent(NpcTextEvent e)
    {
        var textEvent = new TextEvent(e.TextId, TextLocation.PortraitLeft2, e.NpcId);
        return OnBaseTextEvent(textEvent);
    }

    AlbionTask OnPartyMemberTextEvent(PartyMemberTextEvent e)
    {
        var textEvent = new TextEvent(e.TextId, TextLocation.PortraitLeft, e.MemberId.ToSheet());
        return OnBaseTextEvent(textEvent);
    }

    async AlbionTask OnBaseTextEvent(TextEvent mapTextEvent)
    {
        AlbionTask? conversationResult = Conversation?.OnText(mapTextEvent); // Check for custom handling in the current conversation context, if any
        if (conversationResult.HasValue)
        {
            await conversationResult.Value;
            return;
        }

        // Default handling

        var tf = Resolve<ITextFormatter>();
        switch (mapTextEvent.Location)
        {
            case TextLocation.NoPortrait:
                {
                    await AttachChild(new TextDialog(tf.Format(mapTextEvent.ToId(ContextTextSource)))).Task;
                    return;
                }

            case TextLocation.PortraitLeft:
            case TextLocation.PortraitLeft2:
            case TextLocation.PortraitLeft3:
                {
                    var portraitId = GetPortrait(mapTextEvent.Speaker);
                    var text = tf.Ink(Base.Ink.Yellow).Format(mapTextEvent.ToId(ContextTextSource));
                    await AttachChild(new TextDialog(text, portraitId)).Task;
                    return;
                }

            case TextLocation.QuickInfo:
                await RaiseAsync(new DescriptionTextEvent(tf.Format(mapTextEvent.ToId(ContextTextSource))));
                return;

            case TextLocation.Conversation:
            case TextLocation.ConversationQuery:
            case TextLocation.ConversationOptions:
            case TextLocation.StandardOptions:
                break; // Handled by Conversation

            default:
                await RaiseAsync(new DescriptionTextEvent(tf.Format(mapTextEvent.ToId(ContextTextSource)))); // TODO:
                return;
        }
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

    async AlbionTask StartDialogue(StartDialogueEvent e)
    {
        var party = Resolve<IParty>();
        var npc = Assets.LoadSheet(e.NpcId);
        if (npc == null)
        {
            ApiUtil.Assert($"Could not load NPC {e.NpcId}");
            return;
        }

        var wasRunning = Resolve<IClock>().IsRunning;
        if (wasRunning)
            await RaiseAsync(new StopClockEvent());

        Conversation = AttachChild(new Conversation(party?.Leader.Id ?? Base.PartyMember.Tom, npc));
        await Conversation.Run();
        Conversation.Remove();
        Conversation = null;

        if (wasRunning)
            await RaiseAsync(new StartClockEvent());
    }

    async AlbionTask StartPartyDialogue(StartPartyDialogueEvent e)
    {
        var sheet = Assets.LoadSheet(e.MemberId.ToSheet());
        if (sheet == null)
        {
            Error($"Could not load NPC info for \"{e.MemberId}\"");
            return;
        }

        Conversation = AttachChild(new Conversation(Base.PartyMember.Tom, sheet));
        await Conversation.Run();
        Conversation = null;
    }
}
