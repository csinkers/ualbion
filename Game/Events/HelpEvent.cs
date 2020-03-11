using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("help", "Display help on the available console commands.", new[] {"?", "usage"})]
    public class HelpEvent : GameEvent
    {
        public HelpEvent(string commandName)
        {
            CommandName = commandName;
        }

        [EventPart("command", true)]
        public string CommandName { get; }
    }
}
