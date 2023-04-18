using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("focus_console", "Gives the keyboard focus to the console window (if shown)")]
public class FocusConsoleEvent : Event { }