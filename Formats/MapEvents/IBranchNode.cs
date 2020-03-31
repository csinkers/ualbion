namespace UAlbion.Formats.MapEvents
{
    public interface IBranchNode : IEventNode
    {
        IEventNode NextEventWhenFalse { get; set; }
    }
}