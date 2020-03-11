using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("ambient")]
    public class AmbientEvent : GameEvent
    {
        public AmbientEvent(int unk) { Unk = unk; }
        [EventPart("unk")] public int Unk { get; }
    }
}
