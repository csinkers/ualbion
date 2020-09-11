namespace UAlbion.Formats.MapEvents
{
    public interface IBranchNode : IEventNode
    {
        IEventNode NextIfFalse { get; }
    }
}
