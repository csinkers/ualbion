using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

public interface ILockedInventoryEvent : IBranchingEvent
{
    byte PickDifficulty { get; }
    ItemId Key { get; }
    byte OpenedText { get; }
    byte UnlockedText { get; }
}