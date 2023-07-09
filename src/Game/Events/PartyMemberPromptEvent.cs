using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

public class PartyMemberPromptEvent : GameEvent, IAsyncEvent<PartyMemberId>
{
    public StringId Prompt { get; }
    public PartyMemberId[] Members { get; }

    public PartyMemberPromptEvent(TextId prompt, PartyMemberId[] members = null) : this(new StringId(prompt), members) { }
    public PartyMemberPromptEvent(StringId prompt, PartyMemberId[] members = null)
    {
        Prompt = prompt;
        Members = members;
    }
}