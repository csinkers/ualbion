using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("e:toggle_hw_cursor", "Toggles displaying the default windows cursor")] public class ToggleHardwareCursorEvent : EngineEvent { }