using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events;

public record EventVisitedEvent(EventSetId Id, ActionEvent Action) : EventRecord, IVerboseEvent;