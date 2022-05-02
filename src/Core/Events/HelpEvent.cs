using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("help", "Display help on the available console commands.", "?", "usage")]
public class HelpEvent : Event
{
    public HelpEvent(string commandName)
    {
        CommandName = commandName;
    }

    [EventPart("command", true)]
    public string CommandName { get; }
}