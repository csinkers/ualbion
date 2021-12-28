using UAlbion.Api;

namespace UAlbion.Core.Events;

[Event("who", "Display which components handle events of a given type")]
public class WhoEvent : Event
{
    public WhoEvent(string commandName) => CommandName = commandName;
    [EventPart("command", "A regex to find the events of interest")] public string CommandName { get; }
}