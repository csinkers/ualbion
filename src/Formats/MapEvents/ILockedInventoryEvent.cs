using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    public interface ILockedInventoryEvent : ISetInventoryModeEvent, IBranchingEvent, ITextEvent
    {
        byte PickDifficulty { get; }
        ItemId KeyItemId { get; }
        byte InitialTextId { get; }
        byte UnlockedTextId { get; }
    }
}
