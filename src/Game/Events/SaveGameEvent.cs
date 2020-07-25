using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("save_game", "Save the game")]
    public class SaveGameEvent : GameEvent
    {
        public SaveGameEvent(ushort id, string name)
        {
            Id = id;
            Name = name;
        }

        [EventPart("id", "The slot number to save to")]
        public ushort Id { get; }

        [EventPart("name")]
        public string Name { get; }
    }
}
