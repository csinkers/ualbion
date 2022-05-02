using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("cls", "Clear the console history.")]
public class ClearConsoleEvent : Event { }