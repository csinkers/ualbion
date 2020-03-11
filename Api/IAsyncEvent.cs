namespace UAlbion.Api
{
    public interface IAsyncEvent : IEvent
    {
        bool Acknowledged { get; set; }
        void Complete();
    }
}
