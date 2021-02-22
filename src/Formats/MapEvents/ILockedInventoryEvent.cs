using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    public interface ILockedInventoryEvent : IBranchingEvent, ITextEvent
    {
        byte PickDifficulty { get; }
        ItemId Key { get; }
        byte OpenedText { get; }
        byte UnlockedText { get; }
    }
}
