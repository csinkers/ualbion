using UAlbion.Api.Eventing;

namespace UAlbion.Api.Settings;

[Event("save", "Save the current settings to disk")]
public record SaveSettingsEvent : EventRecord;