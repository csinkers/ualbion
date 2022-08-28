using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid.Events;

public record WindowClosedEvent : EventRecord; // Emitted after the window has been closed
public record WindowHiddenEvent : EventRecord;
public record WindowShownEvent : EventRecord;