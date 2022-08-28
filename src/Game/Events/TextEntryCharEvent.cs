using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

public record TextEntryCharEvent(char Character) : EventRecord;