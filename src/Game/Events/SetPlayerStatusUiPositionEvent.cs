using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events;

public class SetPlayerStatusUiPositionEvent : GameEvent, IVerboseEvent
{
    public PartyMemberId Id { get; }
    public int CentreX { get; }
    public int CentreY { get; }

    public SetPlayerStatusUiPositionEvent(PartyMemberId id, int centreX, int centreY)
    {
        Id = id;
        CentreX = centreX;
        CentreY = centreY;
    }
}