using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

public class PartyMemberPromptEvent : GameEvent, IAsyncEvent<PartyMemberId>
{
    public StringId Prompt { get; }
    public PartyMemberId[] Members { get; }

    public PartyMemberPromptEvent(StringId prompt, PartyMemberId[] members = null)
    {
        Prompt = prompt;
        Members = members;
    }
}