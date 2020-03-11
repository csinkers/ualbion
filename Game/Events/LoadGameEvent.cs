using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("load_game", "Load a saved game")]
    public class LoadGameEvent : GameEvent
    {
        public LoadGameEvent(string filename)
        {
            Filename = filename;
        }

        [EventPart("filename")]
        public string Filename { get; }
    }
}
