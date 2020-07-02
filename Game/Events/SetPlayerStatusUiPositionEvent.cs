using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    public class SetPlayerStatusUiPositionEvent : GameEvent, IVerboseEvent
    {
        public PartyCharacterId Id { get; }
        public int CentreX { get; }
        public int CentreY { get; }

        public SetPlayerStatusUiPositionEvent(PartyCharacterId id, int centreX, int centreY)
        {
            Id = id;
            CentreX = centreX;
            CentreY = centreY;
        }
    }
}