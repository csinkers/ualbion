using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("respond", "Chooses the given conversation option")]
    public class RespondEvent : GameEvent
    {
        public RespondEvent(int option) => Option = option;
        [EventPart("option")] public int Option { get; }
    }
}