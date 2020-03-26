namespace UAlbion.Formats.MapEvents
{
    public interface IEventNode
    {
        int Id { get; }
        IMapEvent Event { get; }
        IEventNode NextEvent { get; set; }
    }
}