using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("save_game", "Save the game")]
    public class SaveGameEvent : GameEvent
    {
        public SaveGameEvent(string filename, string name)
        {
            Filename = filename;
            Name = name;
        }

        [EventPart("filename")]
        public string Filename { get; }
        [EventPart("name")]
        public string Name { get; }
    }
}
