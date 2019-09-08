using UAlbion.Api;
using UAlbion.Game.State;

namespace UAlbion.Game.Events
{
    [Event("update")]
    public class UpdateEvent : GameEvent, IVerboseEvent
    {
        public UpdateEvent(int frames) { Frames = frames; }
        [EventPart("frames")] public int Frames { get; }
    }

    public class PostUpdateEvent : GameEvent, IVerboseEvent
    {
        public PostUpdateEvent(IState gameState) { GameState = gameState; }
        public IState GameState { get; }
    }
}