namespace UAlbion.Api
{
    public interface IEvent { }

    public interface IPositionedEvent : IEvent
    {
        IPositionedEvent OffsetClone(int x, int y);
        int X { get; }
        int Y { get; }
    }
}